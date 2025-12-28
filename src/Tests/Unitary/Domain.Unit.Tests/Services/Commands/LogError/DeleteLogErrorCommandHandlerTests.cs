using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using Services.Features.LogError.Commands.DeleteLogError;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class DeleteLogErrorCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly LogErrorCommandFixture _commandFixture;
    private readonly LogErrorFixture _logErrorFixture;

    public DeleteLogErrorCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _commandFixture = new LogErrorCommandFixture();
        _logErrorFixture = new LogErrorFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSoftDeleteLogError()
    {
        // Arrange
        var existingLogError = _logErrorFixture.GenerateLogError();
        await _context.LogErrors.AddAsync(existingLogError);
        await _context.SaveChangesAsync();

        var command = new DeleteLogErrorCommand { Id = existingLogError.Id };
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteLogErrorCommandHandler(
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
        var deletedLogError = await _context.LogErrors.FirstOrDefaultAsync(l => l.Id == existingLogError.Id);
        deletedLogError.Should().NotBeNull();
        deletedLogError!.Deleted.Should().BeTrue();

        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LogErrorNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new DeleteLogErrorCommand { Id = Guid.NewGuid() };

        var handler = new DeleteLogErrorCommandHandler(
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
    public async Task Handle_AlreadyDeletedLogError_ShouldReturnNotFound()
    {
        // Arrange
        var deletedLogError = _logErrorFixture.GenerateDeletedLogError();
        await _context.LogErrors.AddAsync(deletedLogError);
        await _context.SaveChangesAsync();

        var command = new DeleteLogErrorCommand { Id = deletedLogError.Id };

        var handler = new DeleteLogErrorCommandHandler(
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
        var existingLogError = _logErrorFixture.GenerateLogError();
        await _context.LogErrors.AddAsync(existingLogError);
        await _context.SaveChangesAsync();

        var command = new DeleteLogErrorCommand { Id = existingLogError.Id };
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var handler = new DeleteLogErrorCommandHandler(
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
