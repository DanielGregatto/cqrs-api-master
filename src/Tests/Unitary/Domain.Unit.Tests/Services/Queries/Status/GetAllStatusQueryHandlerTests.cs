using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Queries.GetAllStatus;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class GetAllStatusQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly StatusFixture _statusFixture;

    public GetAllStatusQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _statusFixture = new StatusFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_WithMultipleStatuses_ShouldReturnAllNonDeletedStatuses()
    {
        // Arrange
        var statuses = _statusFixture.GenerateStatusList(5);
        var deletedStatus = _statusFixture.GenerateDeletedStatus();
        statuses.Add(deletedStatus);

        await _context.SeedStatusData(statuses);

        var query = new GetAllStatusQuery();

        var handler = new GetAllStatusQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(5); // Should exclude deleted status
        result.Data.Should().OnlyContain(s => !s.Id.Equals(deletedStatus.Id));
    }

    [Fact]
    public async Task Handle_WithNoStatuses_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllStatusQuery();

        var handler = new GetAllStatusQueryHandler(
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
    public async Task Handle_WithOnlyDeletedStatuses_ShouldReturnEmptyList()
    {
        // Arrange
        var deletedStatuses = new List<Domain.Status>
        {
            _statusFixture.GenerateDeletedStatus(),
            _statusFixture.GenerateDeletedStatus(),
            _statusFixture.GenerateDeletedStatus()
        };

        await _context.SeedStatusData(deletedStatuses);

        var query = new GetAllStatusQuery();

        var handler = new GetAllStatusQueryHandler(
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
