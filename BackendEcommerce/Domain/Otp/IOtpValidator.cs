namespace BackendEcommerce.Domain.Otp
{
    public interface IOtpValidator
    {
        Task<bool> VerifyAsync(string? email, string? otp);
    }
}
