using Xunit;

namespace LendingSystem.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestCollectionFixture>
{
    [Obsolete]
    public const string Name = "Integration tests";

    /// <summary>
    /// 用於純讀測試
    /// </summary>
    public const string Readonly = "Readonly";

    public const string Write = "Write";
}
