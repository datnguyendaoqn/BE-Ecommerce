﻿namespace BackendEcommerce.Domain.Contracts.Caching
{
    public interface IOtpRepository
    {
        Task SaveOtpAsync(string email, string otp, TimeSpan expire);
        Task<string?> GetOtpAsync(string email);
        Task DeleteOtpAsync(string email);
    }

}
