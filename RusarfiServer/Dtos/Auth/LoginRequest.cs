using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Dtos.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; init; } = string.Empty;
}
