using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.DTO.Responses;
using Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Commands.CreateStatus;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class CreateStatusCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<ILogger<CreateStatusCommandHandler>> _mockLogger;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly CreateStatusCommandValidator _validator;
    private readonly StatusCommandFixture _commandFixture;

    public CreateStatusCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUser = new Mock<IUser>();
        _mockLogger = new Mock<ILogger<CreateStatusCommandHandler>>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new CreateStatusCommandValidator(_mockLocalizer.Object);
        _commandFixture = new StatusCommandFixture();

        // Setup default user as authenticated
        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateStatus()
    {
        // Arrange
        var command = _commandFixture.GenerateCreateCommand();
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _validator,
            _mockLogger.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Nome.Should().Be(command.Nome);
        result.Data.Escopo.Should().Be(command.Escopo);
        result.Data.Descricao.Should().Be(command.Descricao);
        result.Data.Ativo.Should().Be(command.Ativo);
        result.Data.Bloquear.Should().Be(command.Bloquear);

        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ShouldReturnValidationErrors()
    {
        // Arrange
        var command = _commandFixture.GenerateInvalidCreateCommand();

        var handler = new CreateStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _validator,
            _mockLogger.Object,
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


    [Fact]
    public async Task Handle_DatabaseFailure_ShouldReturnFailure()
    {
        // Arrange
        var command = _commandFixture.GenerateCreateCommand();
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var handler = new CreateStatusCommandHandler(
            _context,
            _mockUser.Object,
            _mockUnitOfWork.Object,
            _validator,
            _mockLogger.Object,
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
