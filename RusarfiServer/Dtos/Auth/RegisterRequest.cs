using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Dtos.Auth;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener mínimo 8 caracteres")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
    [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación no coinciden")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
