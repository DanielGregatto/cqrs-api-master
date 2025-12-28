using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Commands.DeleteStatus;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class DeleteStatusCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly StatusCommandFixture _commandFixture;
    private readonly StatusFixture _statusFixture;

    public DeleteStatusCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _commandFixture = new StatusCommandFixture();
        _statusFixture = new StatusFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSoftDeleteStatus()
    {
        // Arrange
        var existingStatus = _statusFixture.GenerateStatus();
        await _context.SeedStatusData(new List<Domain.Status> { existingStatus });

        var command = new DeleteStatusCommand { Id = existingStatus.Id };
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify soft delete
        var deletedStatus = await _context.Status.FirstOrDefaultAsync(s => s.Id == existingStatus.Id);
        deletedStatus.Should().NotBeNull();
        deletedStatus!.Deleted.Should().BeTrue();

        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StatusNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new DeleteStatusCommand { Id = Guid.NewGuid() };

        var handler = new DeleteStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyDeletedStatus_ShouldReturnNotFound()
    {
        // Arrange
        var deletedStatus = _statusFixture.GenerateDeletedStatus();
        await _context.SeedStatusData(new List<Domain.Status> { deletedStatus });

        var command = new DeleteStatusCommand { Id = deletedStatus.Id };

        var handler = new DeleteStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task Handle_DatabaseFailure_ShouldReturnFailure()
    {
        // Arrange
        var existingStatus = _statusFixture.GenerateStatus();
        await _context.SeedStatusData(new List<Domain.Status> { existingStatus });

        var command = new DeleteStatusCommand { Id = existingStatus.Id };
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var handler = new DeleteStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
