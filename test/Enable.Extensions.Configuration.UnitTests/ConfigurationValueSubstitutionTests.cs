using AutoFixture;
using FluentAssertions;
using Xunit;

namespace Enable.Extensions.Configuration.UnitTests
{
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
            var sut = CreateSubstitution();

            // Act
            var result = sut.Substitute(configurationValue);

            // Assert
            result.Should().Be(configurationValue);
        }

        [Fact]
        public void Substitution_ShouldReturnSubstitutedString()
        {
            // Arrange
            var machineName = _fixture.Create<string>();
            var sut = CreateSubstitution(machineName: machineName);

            var configurationValue = "Endpoint=sb://sbns-myservicebus-{MachineName}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123";
            var expected = $"Endpoint=sb://sbns-myservicebus-{machineName}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123";

            // Act
            var result = sut.Substitute(configurationValue);

            // Assert
            result.Should().Be(expected);
        }

        private ConfigurationValueSubstitution CreateSubstitution(
            string? branch = null,
            string? branchPath = null,
            string? databaseServer = null,
            string? machineName = null)
            => new(
                branch: branch ?? _fixture.Create<string>(),
                branchPath: branchPath ?? _fixture.Create<string>(),
                databaseServer: databaseServer ?? _fixture.Create<string>(),
                machineName: machineName ?? _fixture.Create<string>());
    }
}
