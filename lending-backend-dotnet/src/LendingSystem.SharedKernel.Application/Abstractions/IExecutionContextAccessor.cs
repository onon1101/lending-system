using ExecutionContext = LendingSystem.SharedKernel.Application.Common.ExecutionContext;

namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IExecutionContextAccessor
{
    ExecutionContext Current { get; }   
}
