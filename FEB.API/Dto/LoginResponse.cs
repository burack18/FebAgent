namespace FEB.API.Dto
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; } // Optional: tell client when token expires
        public string Username { get; set; } = string.Empty; // Optional: return username
    }
}
