namespace Backend.Dtos.Auth
{
    public class LoginRequestDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public required IList<string> Roles { get; set; }
        public required string Name { get; set; }
        public required string ID { get; set; }
    }
}