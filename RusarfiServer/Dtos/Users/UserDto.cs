namespace RusarfiServer.Dtos.Users;

public sealed class UserDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
