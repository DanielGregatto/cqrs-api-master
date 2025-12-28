using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Commands.UpdateStatus;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class UpdateStatusCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly UpdateStatusCommandValidator _validator;
    private readonly StatusCommandFixture _commandFixture;
    private readonly StatusFixture _statusFixture;

    public UpdateStatusCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new UpdateStatusCommandValidator(_mockLocalizer.Object);
        _commandFixture = new StatusCommandFixture();
        _statusFixture = new StatusFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateStatus()
    {
        // Arrange
        var existingStatus = _statusFixture.GenerateStatus();
        await _context.SeedStatusData(new List<Domain.Status> { existingStatus });

        var command = _commandFixture.GenerateUpdateCommand();
        command.Id = existingStatus.Id;

        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(command.Id);
        result.Data.Nome.Should().Be(command.Nome);
        result.Data.Escopo.Should().Be(command.Escopo);
        result.Data.Descricao.Should().Be(command.Descricao);

        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StatusNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = _commandFixture.GenerateUpdateCommand();
        command.Id = Guid.NewGuid(); // Non-existent ID

        var handler = new UpdateStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _validator,
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
    public async Task Handle_DeletedStatus_ShouldReturnNotFound()
    {
        // Arrange
        var deletedStatus = _statusFixture.GenerateDeletedStatus();
        await _context.SeedStatusData(new List<Domain.Status> { deletedStatus });

        var command = _commandFixture.GenerateUpdateCommand();
        command.Id = deletedStatus.Id;

        var handler = new UpdateStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _validator,
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
    public async Task Handle_InvalidCommand_ShouldReturnValidationErrors()
    {
        // Arrange
        var command = _commandFixture.GenerateInvalidUpdateCommand();

        var handler = new UpdateStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
