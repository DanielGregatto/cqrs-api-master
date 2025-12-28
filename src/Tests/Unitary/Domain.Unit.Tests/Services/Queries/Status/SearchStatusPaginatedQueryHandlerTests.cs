using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Status.Queries.SearchStatusPaginated;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class SearchStatusPaginatedQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUser> _mockUser;
    private readonly StatusFixture _statusFixture;

    public SearchStatusPaginatedQueryHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUser = new Mock<IUser>();
        _statusFixture = new StatusFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var statuses = _statusFixture.GenerateStatusList(15);
        await _context.SeedStatusData(statuses);

        var query = new SearchStatusPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 5
        };

        var handler = new SearchStatusPaginatedQueryHandler(
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
        var statuses = _statusFixture.GenerateStatusList(10);
        var activeStatus = _statusFixture.GenerateActiveStatus();
        statuses.Add(activeStatus);

        await _context.SeedStatusData(statuses);

        var query = new SearchStatusPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 10,
            Predicate = s => s.Ativo && !s.Bloquear && s.Escopo == "Geral"
        };

        var handler = new SearchStatusPaginatedQueryHandler(
            _context,
            _mockUser.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().Contain(s => s.Id == activeStatus.Id);
        result.Data.Items.Should().OnlyContain(s => s.Ativo && !s.Bloquear && s.Escopo == "Geral");
    }

    [Fact]
    public async Task Handle_WithOrderByDescending_ShouldOrderCorrectly()
    {
        // Arrange
        var statuses = _statusFixture.GenerateStatusList(5);
        await _context.SeedStatusData(statuses);

        var query = new SearchStatusPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 10,
            OrderByProperty = "CreatedAt",
            OrderByDescending = true
        };

        var handler = new SearchStatusPaginatedQueryHandler(
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
        var statuses = _statusFixture.GenerateStatusList(12);
        await _context.SeedStatusData(statuses);

        var query = new SearchStatusPaginatedQuery
        {
            PageIndex = 2,
            PageSize = 5
        };

        var handler = new SearchStatusPaginatedQueryHandler(
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
    public async Task Handle_ShouldExcludeDeletedStatuses()
    {
        // Arrange
        var statuses = _statusFixture.GenerateStatusList(5);
        var deletedStatus = _statusFixture.GenerateDeletedStatus();
        statuses.Add(deletedStatus);

        await _context.SeedStatusData(statuses);

        var query = new SearchStatusPaginatedQuery
        {
            PageIndex = 1,
            PageSize = 10
        };

        var handler = new SearchStatusPaginatedQueryHandler(
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
        result.Data.Items.Should().NotContain(s => s.Id == deletedStatus.Id);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
