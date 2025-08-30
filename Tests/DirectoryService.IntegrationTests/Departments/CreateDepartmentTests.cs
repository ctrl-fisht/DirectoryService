using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Contracts.Departments.Create;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Errors;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DSTestBase
{
    public CreateDepartmentTests(DSWebFactory factory) : base(factory)
    { }
    
    [Fact]
    public async Task CreateDepartment__with_valid_data__should_succeed()
    {
        // arrange
        var locationId = await CreateOneLocation();
        
        // act
        var cancellationToken = CancellationToken.None;
        var result = await ExecuteHandlerAsync<CreateDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new CreateDepartmentCommand()
            {
                Request = new CreateDepartmentRequest()
                {
                    Name = "Главный офис",
                    Identifier = "main",
                    Locations = [locationId],
                    ParentId = null,
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        
        result.IsSuccess.Should().BeTrue();
        
        var resultInDb = await ExecuteInDb(async (dbContext) =>
        {
            return dbContext.Departments.First(d => d.Id == result.Value);
        });

        resultInDb.Id.Should().Be(result.Value);
    }
    
    [Fact]
    public async Task CreateDepartment__with_invalid_inputs__should_failed()
    {
        // arrange
        var locationId = await CreateOneLocation();
        
        // act
        var cancellationToken = CancellationToken.None;
        var result = await ExecuteHandlerAsync<CreateDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new CreateDepartmentCommand()
            {
                Request = new CreateDepartmentRequest()
                {
                    Name = "Главный офис",
                    Identifier = "main....as.d.",
                    Locations = [locationId],
                    ParentId = null,
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public async Task CreateDepartment__with_existed_identifier__should_failed()
    {
        // arrange
        var locationId = await CreateOneLocation();
        await CreateOneDepartment();
        
        // act
        var cancellationToken = CancellationToken.None;
        var result = await ExecuteHandlerAsync<CreateDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new CreateDepartmentCommand()
            {
                Request = new CreateDepartmentRequest()
                {
                    Name = "Главный офис",
                    Identifier = "main",
                    Locations = [locationId],
                    ParentId = null,
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Where(e => e.Code == "record.already.exists").Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateDepartment__with_parent_doesnt_exist__should_failed()
    {
        // arrange
        var locationId = await CreateOneLocation();
        
        // act
        var cancellationToken = CancellationToken.None;
        var result = await ExecuteHandlerAsync<CreateDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new CreateDepartmentCommand()
            {
                Request = new CreateDepartmentRequest()
                {
                    Name = "Главный офис",
                    Identifier = "main",
                    Locations = [locationId],
                    ParentId = Guid.NewGuid(),
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Where(e => e.Code == "record.not.found").Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateDepartment__with_valid_parent__should_succeed()
    {
        // arrange
        var locationId = await CreateOneLocation();
        var departmentId = await CreateOneDepartment();
        
        // act
        var cancellationToken = CancellationToken.None;
        var result = await ExecuteHandlerAsync<CreateDepartmentHandler, Result<Guid, Errors>>(async handler =>
        {
            var command = new CreateDepartmentCommand()
            {
                Request = new CreateDepartmentRequest()
                {
                    Name = "Дочерний офис",
                    Identifier = "child",
                    Locations = [locationId],
                    ParentId = departmentId,
                }
            };
            return await handler.HandleAsync(command, cancellationToken);
        });
        
        // assert
        result.IsSuccess.Should().BeTrue();
        
        var resultInDb = await ExecuteInDb(async (dbContext) =>
        {
            return await dbContext.Departments.FirstAsync(d => d.Id == result.Value, cancellationToken);
        });

        resultInDb.Id.Should().Be(result.Value);
        resultInDb.ParentId.Should().Be(departmentId);
    }
}