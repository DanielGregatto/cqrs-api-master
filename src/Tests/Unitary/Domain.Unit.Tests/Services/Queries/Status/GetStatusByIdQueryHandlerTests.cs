using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Queries.GetStatusById;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class GetStatusByIdQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<ILogger<GetStatusByIdQueryHandler>> _mockLogger;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly StatusFixture _statusFixture;

    public GetStatusByIdQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _mockLogger = new Mock<ILogger<GetStatusByIdQueryHandler>>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _statusFixture = new StatusFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ExistingStatus_ShouldReturnStatus()
    {
        // Arrange
        var existingStatus = _statusFixture.GenerateStatus();
        await _context.SeedStatusData(new List<Domain.Status> { existingStatus });

        var query = new GetStatusByIdQuery { Id = existingStatus.Id };

        var handler = new GetStatusByIdQueryHandler(
            _context,
            _mockUser.Object,
            _mockLogger.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(existingStatus.Id);
        result.Data.Nome.Should().Be(existingStatus.Nome);
        result.Data.Escopo.Should().Be(existingStatus.Escopo);
    }

    [Fact]
    public async Task Handle_NonExistentStatus_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetStatusByIdQuery { Id = Guid.NewGuid() };

        var handler = new GetStatusByIdQueryHandler(
            _context,
            _mockUser.Object,
            _mockLogger.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeletedStatus_ShouldReturnNotFound()
    {
        // Arrange
        var deletedStatus = _statusFixture.GenerateDeletedStatus();
        await _context.SeedStatusData(new List<Domain.Status> { deletedStatus });

        var query = new GetStatusByIdQuery { Id = deletedStatus.Id };

        var handler = new GetStatusByIdQueryHandler(
            _context,
            _mockUser.Object,
            _mockLogger.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
    }


    public void Dispose()
    {
        _context?.Dispose();
    }
}
