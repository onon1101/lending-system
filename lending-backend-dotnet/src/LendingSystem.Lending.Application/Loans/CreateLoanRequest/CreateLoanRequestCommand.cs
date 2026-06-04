using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans.CreateLoanRequest;

public sealed class CreateLoanRequestCommand : ICommand<CreateLoanRequestResult>
{
    /// <summary>
    /// 欲借閱物品的擁有者者名字
    /// </summary>
    [JsonPropertyName("item_owner_username")]
    public string ItemOwnerUsername { get; init; } = string.Empty;

    /// <summary>
    /// 借閱物品
    /// </summary>
    [JsonPropertyName("item_name")]
    public string ItemName { get; init; } = string.Empty;
    
    /// <summary>
    /// 開始借閱時間
    /// </summary>
    public DateOnly StartDate { get; init; }
    
    /// <summary>
    /// 預期借閱時間
    /// </summary>
    [JsonPropertyName("duration_days")]
    public int DurationDays { get; init; }
}
