using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using Services.Features.LogError.Commands.CreateLogError;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class CreateLogErrorCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly CreateLogErrorCommandValidator _validator;
    private readonly LogErrorCommandFixture _commandFixture;

    public CreateLogErrorCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new CreateLogErrorCommandValidator(_mockLocalizer.Object);
        _commandFixture = new LogErrorCommandFixture();

        // Setup default user as authenticated
        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateLogError()
    {
        // Arrange
        var command = _commandFixture.GenerateCreateCommand();
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateLogErrorCommandHandler(
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
        result.Data.Code.Should().Be(command.Code);
        result.Data.Record.Should().Be(command.Record);
        result.Data.UserId.Should().Be(command.UserId);

        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DatabaseFailure_ShouldReturnFailure()
    {
        // Arrange
        var command = _commandFixture.GenerateCreateCommand();
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var handler = new CreateLogErrorCommandHandler(
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
