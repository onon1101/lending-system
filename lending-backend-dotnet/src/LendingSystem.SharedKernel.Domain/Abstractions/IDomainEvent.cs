using MediatR;

namespace LendingSystem.SharedKernel.Domain.Abstractions;

public interface IDomainEvent : INotification
{
   Guid Id { get; } 
   
   DateTime OccurredOn { get; }
}