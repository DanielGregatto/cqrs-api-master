using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Services.Features.LogError.Queries.SearchLogErrorsPaginated;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class SearchLogErrorsPaginatedQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly LogErrorFixture _logErrorFixture;

    public SearchLogErrorsPaginatedQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _logErrorFixture = new LogErrorFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var logErrors = _logErrorFixture.GenerateLogErrorList(15);
        await _context.LogErrors.AddRangeAsync(logErrors);
        await _context.SaveChangesAsync();

        var query = new SearchLogErrorsPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 5
        };

        var handler = new SearchLogErrorsPaginatedQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(5);
        result.Data.PageIndex.Should().Be(1);
        result.Data.PageSize.Should().Be(5);
        result.Data.TotalCount.Should().Be(15);
        result.Data.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithPredicate_ShouldFilterResults()
    {
        // Arrange
        var logErrors = _logErrorFixture.GenerateLogErrorList(10);
        var specificUserId = Guid.NewGuid();
        var userSpecificError = _logErrorFixture.GenerateLogError();
        userSpecificError.UserId = specificUserId;
        logErrors.Add(userSpecificError);

        await _context.LogErrors.AddRangeAsync(logErrors);
        await _context.SaveChangesAsync();

        var query = new SearchLogErrorsPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 20,
            Predicate = l => l.UserId == specificUserId
        };

        var handler = new SearchLogErrorsPaginatedQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().Contain(l => l.Id == userSpecificError.Id);
        result.Data.Items.Should().OnlyContain(l => l.UserId == specificUserId);
    }

    [Fact]
    public async Task Handle_WithOrderByDescending_ShouldOrderCorrectly()
    {
        // Arrange
        var logErrors = _logErrorFixture.GenerateLogErrorList(5);
        await _context.LogErrors.AddRangeAsync(logErrors);
        await _context.SaveChangesAsync();

        var query = new SearchLogErrorsPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 10,
            OrderByProperty = "CreatedAt",
            OrderByDescending = true
        };

        var handler = new SearchLogErrorsPaginatedQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(5);

        // Verify descending order
        var items = result.Data.Items.ToList();
        for (int i = 0; i < items.Count - 1; i++)
        {
            items[i].CreatedAt.Should().BeOnOrAfter(items[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task Handle_SecondPage_ShouldReturnCorrectItems()
    {
        // Arrange
        var logErrors = _logErrorFixture.GenerateLogErrorList(12);
        await _context.LogErrors.AddRangeAsync(logErrors);
        await _context.SaveChangesAsync();

        var query = new SearchLogErrorsPaginatedQuery
        {
            PageIndex = 2,
            PageSize = 5
        };

        var handler = new SearchLogErrorsPaginatedQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(5);
        result.Data.PageIndex.Should().Be(2);
        result.Data.HasNextPage.Should().BeTrue();
        result.Data.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldExcludeDeletedLogErrors()
    {
        // Arrange
        var logErrors = _logErrorFixture.GenerateLogErrorList(5);
        var deletedLogError = _logErrorFixture.GenerateDeletedLogError();
        logErrors.Add(deletedLogError);

        await _context.LogErrors.AddRangeAsync(logErrors);
        await _context.SaveChangesAsync();

        var query = new SearchLogErrorsPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 10
        };

        var handler = new SearchLogErrorsPaginatedQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalCount.Should().Be(5); // Excludes deleted
        result.Data.Items.Should().NotContain(l => l.Id == deletedLogError.Id);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
