using Xunit;

namespace LendingSystem.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestCollectionFixture>
{
    public const string Name = "Integration tests";
}
