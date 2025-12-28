using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Services.Features.LogError.Queries.GetAllLogErrors;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class GetAllLogErrorsQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly LogErrorFixture _logErrorFixture;

    public GetAllLogErrorsQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _logErrorFixture = new LogErrorFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_WithMultipleLogErrors_ShouldReturnAllNonDeletedLogErrors()
    {
        // Arrange
        var logErrors = _logErrorFixture.GenerateLogErrorList(5);
        var deletedLogError = _logErrorFixture.GenerateDeletedLogError();
        logErrors.Add(deletedLogError);

        await _context.LogErrors.AddRangeAsync(logErrors);
        await _context.SaveChangesAsync();

        var query = new GetAllLogErrorsQuery();

        var handler = new GetAllLogErrorsQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(5); // Should exclude deleted logError
        result.Data.Should().OnlyContain(l => !l.Id.Equals(deletedLogError.Id));
    }

    [Fact]
    public async Task Handle_WithNoLogErrors_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllLogErrorsQuery();

        var handler = new GetAllLogErrorsQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithOnlyDeletedLogErrors_ShouldReturnEmptyList()
    {
        // Arrange
        var deletedLogErrors = new List<Domain.LogError>
        {
            _logErrorFixture.GenerateDeletedLogError(),
            _logErrorFixture.GenerateDeletedLogError(),
            _logErrorFixture.GenerateDeletedLogError()
        };

        await _context.LogErrors.AddRangeAsync(deletedLogErrors);
        await _context.SaveChangesAsync();

        var query = new GetAllLogErrorsQuery();

        var handler = new GetAllLogErrorsQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
