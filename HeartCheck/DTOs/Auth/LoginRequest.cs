namespace HeartCheck.DTOs.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string Platform { get; set; } = "Web";
    }
}
