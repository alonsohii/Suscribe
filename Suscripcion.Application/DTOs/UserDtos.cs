using System.ComponentModel.DataAnnotations;

namespace Suscripcion.Application.DTOs;

public record RegisterUserRequestDto(
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre debe tener menos de 100 caracteres")]
    string Name,
    
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email invalido")]
    [StringLength(255, ErrorMessage = "El email debe tener menos de 255 caracteres")]
    string Email
);

public record RegisterUserResponseDto(int UserId, string Message);
