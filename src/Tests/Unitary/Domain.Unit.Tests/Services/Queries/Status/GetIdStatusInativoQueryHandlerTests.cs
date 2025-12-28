using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Queries.GetIdStatusInativo;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class GetIdStatusInativoQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly StatusFixture _statusFixture;

    public GetIdStatusInativoQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _statusFixture = new StatusFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_WithInactiveStatus_ShouldReturnId()
    {
        // Arrange
        var activeStatus = _statusFixture.GenerateActiveStatus();
        var inactiveStatus = _statusFixture.GenerateInactiveStatus();
        await _context.SeedStatusData(new List<Domain.Status> { activeStatus, inactiveStatus });

        var query = new GetIdStatusInativoQuery();

        var handler = new GetIdStatusInativoQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(inactiveStatus.Id);
    }

    [Fact]
    public async Task Handle_WithNoInactiveStatus_ShouldReturnNotFound()
    {
        // Arrange
        var activeStatus = _statusFixture.GenerateActiveStatus();
        await _context.SeedStatusData(new List<Domain.Status> { activeStatus });

        var query = new GetIdStatusInativoQuery();

        var handler = new GetIdStatusInativoQueryHandler(
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
    public async Task Handle_WithDeletedInactiveStatus_ShouldReturnNotFound()
    {
        // Arrange
        var inactiveStatus = _statusFixture.GenerateInactiveStatus();
        inactiveStatus.Deleted = true;
        await _context.SeedStatusData(new List<Domain.Status> { inactiveStatus });

        var query = new GetIdStatusInativoQuery();

        var handler = new GetIdStatusInativoQueryHandler(
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
    public async Task Handle_WithUnblockedStatus_ShouldNotReturnIt()
    {
        // Arrange
        var unblockedStatus = _statusFixture.GenerateInactiveStatus();
        unblockedStatus.Bloquear = false; // Not blocked
        await _context.SeedStatusData(new List<Domain.Status> { unblockedStatus });

        var query = new GetIdStatusInativoQuery();

        var handler = new GetIdStatusInativoQueryHandler(
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
