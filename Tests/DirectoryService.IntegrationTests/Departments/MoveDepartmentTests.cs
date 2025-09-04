using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments.MoveDepartment;
using DirectoryService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Errors;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentTests : DSTestBase
{
    public MoveDepartmentTests(DSWebFactory factory) : base(factory)
    {
    }
    
    // hierarchy
    //          main              sales
    //           |               /      \
    //         moscow       shops     call-center
    //        /       \ 
    //    marketing  engineers
    
    
    [Fact]
    public async Task MoveDepartment__to_another_node__should_succeed()
    {
        // arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        var cancellationToken = CancellationToken.None;
        
        var (main, sales) = await ExecuteInDb(async (dbContext) =>
        {
            var main = await dbContext.Departments
                .FirstAsync(d => d.Identifier == Identifier.Create("main").Value, cancellationToken);

            var sales = await dbContext.Departments
                .FirstAsync(d => d.Identifier == Identifier.Create("sales").Value, cancellationToken);

            return (main, sales);
        });

        // act — переносим sales под main
        var result = await ExecuteHandlerAsync<MoveDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new MoveDepartmentCommand()
            {
                DepartmentId = sales.Id,
                Request = new MoveDepartmentRequest()
                {
                    ParentId = main.Id
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsSuccess.Should().BeTrue();

        var updatedSales = await ExecuteInDb(async (dbContext) =>
        {
            return await dbContext.Departments
                .FirstAsync(d => d.Id == sales.Id, cancellationToken);
        });

        updatedSales.ParentId.Should().Be(main.Id);
        updatedSales.Depth.Should().Be(main.Depth + 1);
        updatedSales.Path.Value.Should().Be("main.sales");
    }
    
    [Fact]
    public async Task MoveDepartment__to_root__should_succeed()
    {
        // arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        var cancellationToken = CancellationToken.None;
        
        var sales = await ExecuteInDb(async (dbContext) =>
        {
            var sales = await dbContext.Departments
                .FirstAsync(d => d.Identifier == Identifier.Create("sales").Value, cancellationToken);

            return sales;
        });

        // act — переносим sales в корень
        var result = await ExecuteHandlerAsync<MoveDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new MoveDepartmentCommand()
            {
                DepartmentId = sales.Id,
                Request = new MoveDepartmentRequest()
                {
                    ParentId = null
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsSuccess.Should().BeTrue();

        var updatedSales = await ExecuteInDb(async (dbContext) =>
        {
            return await dbContext.Departments
                .FirstAsync(d => d.Id == sales.Id, cancellationToken);
        });

        updatedSales.ParentId.Should().Be(null);
        updatedSales.Depth.Should().Be(1);
        updatedSales.Path.Value.Should().Be("sales");
    }
    
    [Fact]
    public async Task MoveDepartment__with_invalid_inputs__should_failed()
    {
        // arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandlerAsync<MoveDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new MoveDepartmentCommand()
            {
                DepartmentId = Guid.Empty,
                Request = new MoveDepartmentRequest()
                {
                    ParentId = Guid.Empty
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        
        result.Error.Count().Should().Be(2);
    }
    
    [Fact]
    public async Task MoveDepartment__to_self_child__should_failed()
    {
        // arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        var cancellationToken = CancellationToken.None;
        
        var (salesShops, sales) = await ExecuteInDb(async (dbContext) =>
        {
            var salesShops = await dbContext.Departments
                .FirstAsync(d => d.Identifier == Identifier.Create("shops").Value, cancellationToken);

            var sales = await dbContext.Departments
                .FirstAsync(d => d.Identifier == Identifier.Create("sales").Value, cancellationToken);

            return (salesShops, sales);
        });

        // act — переносим sales под main
        var result = await ExecuteHandlerAsync<MoveDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new MoveDepartmentCommand()
            {
                DepartmentId = sales.Id,
                Request = new MoveDepartmentRequest()
                {
                    ParentId = salesShops.Id
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Count().Should().Be(1);
        result.Error.First().Code.Contains("cannot.add.ancestor").Should().BeTrue();
    }
    
    [Fact]
    public async Task MoveDepartment__moving_department_not_exist__should_failed()
    {
        // arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        var cancellationToken = CancellationToken.None;

        // act — переносим sales под main
        var result = await ExecuteHandlerAsync<MoveDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new MoveDepartmentCommand()
            {
                DepartmentId = Guid.NewGuid(),
                Request = new MoveDepartmentRequest()
                {
                    ParentId = Guid.NewGuid()
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Count().Should().Be(1);
        result.Error.First().Code.Contains("record.not.found").Should().BeTrue();
    }
    
    [Fact]
    public async Task MoveDepartment__parent_not_exist__should_failed()
    {
        // arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        var cancellationToken = CancellationToken.None;
        
        var sales = await ExecuteInDb(async (dbContext) =>
        {

            var sales = await dbContext.Departments
                .FirstAsync(d => d.Identifier == Identifier.Create("sales").Value, cancellationToken);

            return sales;
        });

        // act — переносим sales под main
        var result = await ExecuteHandlerAsync<MoveDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new MoveDepartmentCommand()
            {
                DepartmentId = sales.Id,
                Request = new MoveDepartmentRequest()
                {
                    ParentId = Guid.NewGuid()
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Count().Should().Be(1);
        result.Error.First().Code.Contains("record.not.found").Should().BeTrue();
    }
}