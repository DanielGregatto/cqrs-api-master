using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Queries.GetIdStatusAtivo;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class GetIdStatusAtivoQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly StatusFixture _statusFixture;

    public GetIdStatusAtivoQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _statusFixture = new StatusFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_WithActiveStatus_ShouldReturnId()
    {
        // Arrange
        var activeStatus = _statusFixture.GenerateActiveStatus();
        var inactiveStatus = _statusFixture.GenerateInactiveStatus();
        await _context.SeedStatusData(new List<Domain.Status> { activeStatus, inactiveStatus });

        var query = new GetIdStatusAtivoQuery();

        var handler = new GetIdStatusAtivoQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(activeStatus.Id);
    }

    [Fact]
    public async Task Handle_WithNoActiveStatus_ShouldReturnNotFound()
    {
        // Arrange
        var inactiveStatus = _statusFixture.GenerateInactiveStatus();
        await _context.SeedStatusData(new List<Domain.Status> { inactiveStatus });

        var query = new GetIdStatusAtivoQuery();

        var handler = new GetIdStatusAtivoQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithDeletedActiveStatus_ShouldReturnNotFound()
    {
        // Arrange
        var activeStatus = _statusFixture.GenerateActiveStatus();
        activeStatus.Deleted = true;
        await _context.SeedStatusData(new List<Domain.Status> { activeStatus });

        var query = new GetIdStatusAtivoQuery();

        var handler = new GetIdStatusAtivoQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithBlockedStatus_ShouldNotReturnIt()
    {
        // Arrange
        var blockedStatus = _statusFixture.GenerateActiveStatus();
        blockedStatus.Bloquear = true; // Blocked
        await _context.SeedStatusData(new List<Domain.Status> { blockedStatus });

        var query = new GetIdStatusAtivoQuery();

        var handler = new GetIdStatusAtivoQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
