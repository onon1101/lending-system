using LendingSystem.SharedKernel.Domain.Abstractions;

namespace LendingSystem.SharedKernel.Domain.Common;

public sealed class BusinessRuleValidationException(IBusinessRule brokenRule) : Exception(brokenRule.Message)
{
    public IBusinessRule BrokenRule { get; } = brokenRule;
}
