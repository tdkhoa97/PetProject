namespace AccountService.Models;

public class RefreshTokenRequestDto
{
    public long UserId { get; set; }
    public required string RefreshToken { get; set; }
}