using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using Services.Features.LogError.Commands.UpdateLogError;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class UpdateLogErrorCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly UpdateLogErrorCommandValidator _validator;
    private readonly LogErrorCommandFixture _commandFixture;
    private readonly LogErrorFixture _logErrorFixture;

    public UpdateLogErrorCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new UpdateLogErrorCommandValidator(_mockLocalizer.Object);
        _commandFixture = new LogErrorCommandFixture();
        _logErrorFixture = new LogErrorFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateLogError()
    {
        // Arrange
        var existingLogError = _logErrorFixture.GenerateLogError();
        await _context.LogErrors.AddAsync(existingLogError);
        await _context.SaveChangesAsync();

        var command = _commandFixture.GenerateUpdateCommand();
        command.Id = existingLogError.Id;

        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateLogErrorCommandHandler(
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
        result.Data.Code.Should().Be(command.Code);
        result.Data.Record.Should().Be(command.Record);

        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LogErrorNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = _commandFixture.GenerateUpdateCommand();
        command.Id = Guid.NewGuid(); // Non-existent ID

        var handler = new UpdateLogErrorCommandHandler(
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
    public async Task Handle_DeletedLogError_ShouldReturnNotFound()
    {
        // Arrange
        var deletedLogError = _logErrorFixture.GenerateDeletedLogError();
        await _context.LogErrors.AddAsync(deletedLogError);
        await _context.SaveChangesAsync();

        var command = _commandFixture.GenerateUpdateCommand();
        command.Id = deletedLogError.Id;

        var handler = new UpdateLogErrorCommandHandler(
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
    public async Task Handle_DatabaseFailure_ShouldReturnFailure()
    {
        // Arrange
        var existingLogError = _logErrorFixture.GenerateLogError();
        await _context.LogErrors.AddAsync(existingLogError);
        await _context.SaveChangesAsync();

        var command = _commandFixture.GenerateUpdateCommand();
        command.Id = existingLogError.Id;

        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var handler = new UpdateLogErrorCommandHandler(
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
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
