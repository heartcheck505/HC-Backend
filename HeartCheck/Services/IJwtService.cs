namespace HeartCheck.Services
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email, string role);
        string GenerateToken(string userId, string email, string role, string platform, string sessionId);
    }
}
