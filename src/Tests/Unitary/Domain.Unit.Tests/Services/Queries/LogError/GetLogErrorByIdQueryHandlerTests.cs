using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using Services.Features.LogError.Queries.GetLogErrorById;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class GetLogErrorByIdQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly LogErrorFixture _logErrorFixture;

    public GetLogErrorByIdQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _logErrorFixture = new LogErrorFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ExistingLogError_ShouldReturnLogError()
    {
        // Arrange
        var existingLogError = _logErrorFixture.GenerateLogError();
        await _context.LogErrors.AddAsync(existingLogError);
        await _context.SaveChangesAsync();

        var query = new GetLogErrorByIdQuery { Id = existingLogError.Id };

        var handler = new GetLogErrorByIdQueryHandler(
            _context,
            _mockUser.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(existingLogError.Id);
        result.Data.Code.Should().Be(existingLogError.Code);
        result.Data.Record.Should().Be(existingLogError.Record);
    }

    [Fact]
    public async Task Handle_NonExistentLogError_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetLogErrorByIdQuery { Id = Guid.NewGuid() };

        var handler = new GetLogErrorByIdQueryHandler(
            _context,
            _mockUser.Object,
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
    public async Task Handle_DeletedLogError_ShouldReturnNotFound()
    {
        // Arrange
        var deletedLogError = _logErrorFixture.GenerateDeletedLogError();
        await _context.LogErrors.AddAsync(deletedLogError);
        await _context.SaveChangesAsync();

        var query = new GetLogErrorByIdQuery { Id = deletedLogError.Id };

        var handler = new GetLogErrorByIdQueryHandler(
            _context,
            _mockUser.Object,
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
