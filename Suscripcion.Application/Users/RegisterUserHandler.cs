using Suscripcion.Application.DTOs;
using Suscripcion.Domain.Entities;
using Suscripcion.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace Suscripcion.Application.Users;

public class RegisterUserHandler
{
    private readonly ISuscripcionRepository _repository;

    public RegisterUserHandler(ISuscripcionRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegisterUserResponseDto> Handle(RegisterUserRequestDto request)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(request.Name))
            return new RegisterUserResponseDto(0, "El nombre es requerido");

        if (request.Name.Length > 100)
            return new RegisterUserResponseDto(0, "El nombre debe tener menos de 100 caracteres");

        if (string.IsNullOrWhiteSpace(request.Email))
            return new RegisterUserResponseDto(0, "El email es requerido");

        if (!IsValidEmail(request.Email))
            return new RegisterUserResponseDto(0, "Formato de email invalido");

        if (request.Email.Length > 255)
            return new RegisterUserResponseDto(0, "El email debe tener menos de 255 caracteres");

        // Si el email ya existe, retorna el usuario existente
        var existingUser = await _repository.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
            return new RegisterUserResponseDto(existingUser.Id, "Usuario ya registrado");

        var user = new User(request.Name, request.Email);
        _repository.AddUser(user);
        await _repository.SaveChangesAsync();

        return new RegisterUserResponseDto(user.Id, "Usuario registrado exitosamente");
    }

    private bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }
}
