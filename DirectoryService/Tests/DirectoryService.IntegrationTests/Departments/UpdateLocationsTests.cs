using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts.Departments.UpdateLocations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Errors;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateLocationsTests : DSTestBase
{
    public UpdateLocationsTests(DSWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateLocations__with_valid_data__should_succeed()
    {
        // arrange
        var department = await CreateDepartmentWithLocations();
        var newLocations = await CreateManyLocations();

        var cancellationToken = CancellationToken.None;
        var oldLocationIds = department.DepartmentLocations.Select(dl => dl.LocationId).ToList();
        // act
        var result = await ExecuteHandlerAsync<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(async (handler) =>
        {
            var command = new UpdateDepartmentLocationsCommand()
            {
                DepartmentId = department.Id,
                Request = new UpdateDepartmentLocationsRequest()
                {
                    LocationIds = newLocations
                }
            };

            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsSuccess.Should().BeTrue();

        var departmentDb = await ExecuteInDb(async (dbContext) =>
        {
           return await dbContext.Departments.Where(d => d.Id == department.Id)
                .Include(d => d.DepartmentLocations)
                .FirstAsync(cancellationToken);
        });

        var addedIds = departmentDb.DepartmentLocations.Select(dl => dl.LocationId).ToList();
        addedIds.Count.Should().Be(newLocations.Count);
        addedIds.Should().BeEquivalentTo(newLocations);
        addedIds.Should().NotContain(oldLocationIds);
    }
    
    [Fact]
    public async Task UpdateLocations__with_invalid_inputs__should_failed()
    {
        // arrange
        var newLocations = await CreateManyLocations();

        var cancellationToken = CancellationToken.None;
        // act
        var result = await ExecuteHandlerAsync<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(async (handler) =>
        {
            var command = new UpdateDepartmentLocationsCommand()
            {
                DepartmentId = Guid.Empty,
                Request = new UpdateDepartmentLocationsRequest()
                {
                    LocationIds = [newLocations.First(), newLocations.First()]
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
    public async Task UpdateLocations__with_invalid_department_id__should_failed()
    {
        // arrange
        var newLocations = await CreateManyLocations();

        var cancellationToken = CancellationToken.None;
        // act
        var result = await ExecuteHandlerAsync<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(async (handler) =>
        {
            var command = new UpdateDepartmentLocationsCommand()
            {
                DepartmentId = Guid.NewGuid(),
                Request = new UpdateDepartmentLocationsRequest()
                {
                    LocationIds = newLocations
                }
            };

            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public async Task UpdateLocations__with_nonexistent_locations__should_failed()
    {
        // arrange
        var department = await CreateDepartmentWithLocations();

        var cancellationToken = CancellationToken.None;
        // act
        var result = await ExecuteHandlerAsync<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(async (handler) =>
        {
            var command = new UpdateDepartmentLocationsCommand()
            {
                DepartmentId = department.Id,
                Request = new UpdateDepartmentLocationsRequest()
                {
                    LocationIds = [Guid.NewGuid(), Guid.NewGuid()]
                }
            };

            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}