using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Test.Services;

public class RedisCacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<RedisCacheService>> _mockLogger;
    private readonly RedisCacheService _service;

    public RedisCacheServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<RedisCacheService>>();
        _service = new RedisCacheService(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_WhenValueDoesNotExist_ReturnsDefault()
    {
        // Arrange
        var key = "nonexistent:key";
        _mockCache.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_CallsDistributedCache()
    {
        // Arrange
        var key = "test:key";
        var value = new { Name = "Test" };

        // Act
        await _service.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_CallsDistributedCache()
    {
        // Arrange
        var key = "test:key";

        // Act
        await _service.RemoveAsync(key);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ReturnsTrue()
    {
        // Arrange
        var key = "test:key";
        _mockCache.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("data"));

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var key = "nonexistent:key";
        _mockCache.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }
}