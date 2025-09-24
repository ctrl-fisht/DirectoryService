using Shared.Core.Database;
using DirectoryService.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.CleanupBackgroundService;

public class CleanupDepartmentsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CleanupDepartmentsOptions _options;
    private readonly ILogger<CleanupDepartmentsBackgroundService> _logger;
    
    public CleanupDepartmentsBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<CleanupDepartmentsOptions> options,
        ILogger<CleanupDepartmentsBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanInactives(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
           
            TimeSpan delay;

            if (_options.IntervalHours == 24 &&
                !string.IsNullOrWhiteSpace(_options.DailyTimeUtc) &&
                TimeSpan.TryParse(_options.DailyTimeUtc, out var dailyTime))
            {
                // Расчёт времени до следующего запуска в точное время
                var now = DateTime.UtcNow;
                var nextRun = now.Date + dailyTime;
                if (nextRun <= now) nextRun = nextRun.AddDays(1);
                delay = nextRun - now;
            }
            else
            {
                // Интервал через часы
                delay = TimeSpan.FromHours(_options.IntervalHours);
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task CleanInactives(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var transactionOpenResult = await transactionManager.BeginTransactionAsync(stoppingToken);
        if (transactionOpenResult.IsFailure)
        {
            _logger.LogError("Error while opening transaction");
            return;
        }

        var transaction = transactionOpenResult.Value;
        
        // Отнимаем от текущей даты 30 дней и получаем условие для выборки
        var cutoff = DateTime.UtcNow - TimeSpan.FromDays(_options.InactiveDays);
        
        FormattableString updateSql =
             $"""
             WITH to_clean AS (
                 SELECT id, path, identifier
                 FROM departments
                 WHERE is_active = false
                   AND deleted_at IS NOT NULL
                   AND deleted_at < {cutoff}
             )
             UPDATE departments child
             SET path = REPLACE(child.path::text, '.' || parent.identifier::text, '')::ltree
             FROM to_clean parent
             WHERE child.path <@ parent.path;
             """;

        var updatedPaths = await dbContext.Database.ExecuteSqlInterpolatedAsync(updateSql, stoppingToken);
        
        FormattableString updateParentIdSql = 
            $"""
            WITH to_clean AS (
                SELECT id, path
                FROM departments
                WHERE is_active = false
                  AND deleted_at IS NOT NULL
                  AND deleted_at < {cutoff}
            )
            UPDATE departments child
            SET parent_id = (
                SELECT id
                FROM departments new_parent
                    WHERE child.path <@ new_parent.path
                    AND new_parent.path <@ child.path
                ORDER BY nlevel(new_parent.path) DESC
                LIMIT 1
            )
            FROM to_clean parent
            WHERE child.path <@ parent.path
              AND child.id <> parent.id;
            """;
        
        var updatedParents = await dbContext.Database.ExecuteSqlInterpolatedAsync(updateParentIdSql, stoppingToken);
        
        FormattableString deleteSql = 
             $"""
             WITH to_clean AS (
                 SELECT id
                 FROM departments
                 WHERE is_active = false
                   AND deleted_at IS NOT NULL
                   AND deleted_at < {cutoff}
             )
             DELETE FROM departments
             WHERE id IN (SELECT id FROM to_clean);
             """;
        
        var deletedCount = await dbContext.Database.ExecuteSqlInterpolatedAsync(deleteSql, stoppingToken);

        var commitResult = transaction.Commit(stoppingToken);
        if (commitResult.IsFailure)
        {
            _logger.LogError("Error while commiting transaction");
            return;
        }
        
        _logger.LogInformation(
            "Successfully deleted {@DeletedCount} inactive departments, Updated {@UpdatedChildren} children",
            deletedCount,
            updatedPaths);
    }
}