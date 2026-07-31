using System.ComponentModel.DataAnnotations;

namespace LendingSystem.Auth.WebApi.Register;

public sealed class RegisterRequest(
    [Required]
    string Account,
    
    [Required]
    string Email,
    
    [Required]
    string Password);