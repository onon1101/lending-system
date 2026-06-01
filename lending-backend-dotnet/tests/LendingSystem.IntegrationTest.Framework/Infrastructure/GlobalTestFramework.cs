using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework(
    "LendingSystem.IntegrationTest.Framework.Infrastructure.GlobalTestFramework",
    "LendingSystem.IntegrationTest.Framework")]

namespace LendingSystem.IntegrationTest.Framework.Infrastructure;

public class GlobalTestFramework : XunitTestFramework
{
    public GlobalTestFramework(IMessageSink messageSink) : base(messageSink)
    {
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
    {
        IntegrationTestDatabase.UseAssemblyDatabaseSuffix(assemblyName.Name ?? string.Empty);
        return new GlobalTestFrameworkExecutor(
            assemblyName,
            SourceInformationProvider,
            DiagnosticMessageSink);
    }

}

// 利用 xUnit 啟動點進行非同步全域初始化
public class GlobalTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public GlobalTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink) 
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
        // 封鎖並同步初始化所有資料庫 Pool
        IntegrationTestDatabase.UseAssemblyDatabaseSuffix(assemblyName.Name ?? string.Empty);
        IntegrationTestDatabase.GlobalInitializeAsync().GetAwaiter().GetResult();
    }
}
