﻿namespace KSMS.Domain.Dtos.Responses;

public class LoginResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    
}