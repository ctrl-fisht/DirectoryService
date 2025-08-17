using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.EfCore.Configurations;

public class DepartmentPositions : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions", tb =>
        {

        });

        builder.HasKey(dp => new { dp.DepartmentId, dp.PositionId });
        
        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id");
        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id");
    }
}