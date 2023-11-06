using AutoFixture;
using FluentAssertions;
using Xunit;

namespace Enable.Extensions.Configuration.UnitTests;

public class ConfigurationValueSubstitutionTests
{
    private readonly Fixture _fixture = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("foo")]
    [InlineData("Endpoint=sb://sbns-myservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123")]
    public void Substitution_ShouldReturnStringWithNoPlaceholdersAsIs(string configurationValue)
    {
        // Arrange
        var sut = CreateSubstitution(branch: null as string);

        // Act
        var result = sut.Substitute(configurationValue);

        // Assert
        result.Should().Be(configurationValue);
    }

    [Fact]
    public void Substitution_ShouldReturnSubstitutedString()
    {
        // Arrange
        var branch = _fixture.Create<string>();
        var branchPath = _fixture.Create<string>();
        var databaseServer = _fixture.Create<string>();
        var machineName = _fixture.Create<string>();
        var serviceBusSharedAccessKey = _fixture.Create<string>();

        var sut = CreateSubstitution(
            branch: branch,
            branchPath: branchPath,
            databaseServer: databaseServer,
            machineName: machineName,
            serviceBusSharedAccessKey: serviceBusSharedAccessKey);

        var configurationValue = "Branch={Branch};BranchPath={BranchPath};DatabaseServer={DatabaseServer};MachineName={MachineName};ServiceBusSharedAccessKey={ServiceBusSharedAccessKey}";
        var expected = $"Branch={branch};BranchPath={branchPath};DatabaseServer={databaseServer};MachineName={machineName};ServiceBusSharedAccessKey={serviceBusSharedAccessKey}";

        // Act
        var result = sut.Substitute(configurationValue);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Substitution_DoesNotInvokeLazyUnnecessarily()
    {
        // Arrange
        var lazyInvoked = false;
        var spy = new Lazy<string>(() =>
        {
            lazyInvoked = true;
            return _fixture.Create<string>();
        });

        var sut = CreateSubstitution(
            branch: spy,
            branchPath: spy,
            databaseServer: spy);

        // Act
        var result = sut.Substitute(settingValue: _fixture.Create<string>());

        // Assert
        lazyInvoked.Should().BeFalse();
    }

    private static Lazy<string> CreateLazyString(string value) => new(() => value);

    private ConfigurationValueSubstitution CreateSubstitution(
        Lazy<string>? branch = null,
        Lazy<string>? branchPath = null,
        Lazy<string>? databaseServer = null,
        string? machineName = null,
        string? serviceBusSharedAccessKey = null)
        => new(
            branch: branch ?? CreateLazyString(_fixture.Create<string>()),
            branchPath: branchPath ?? CreateLazyString(_fixture.Create<string>()),
            databaseServer: databaseServer ?? CreateLazyString(_fixture.Create<string>()),
            machineName: machineName ?? _fixture.Create<string>(),
            serviceBusSharedAccessKey: serviceBusSharedAccessKey ?? _fixture.Create<string>());

    private ConfigurationValueSubstitution CreateSubstitution(
        string? branch = null,
        string? branchPath = null,
        string? databaseServer = null,
        string? machineName = null,
        string? serviceBusSharedAccessKey = null)
        => CreateSubstitution(
            branch: CreateLazyString(branch ?? _fixture.Create<string>()),
            branchPath: CreateLazyString(branchPath ?? _fixture.Create<string>()),
            databaseServer: CreateLazyString(databaseServer ?? _fixture.Create<string>()),
            machineName: machineName,
            serviceBusSharedAccessKey: serviceBusSharedAccessKey);
}
