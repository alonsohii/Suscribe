using Microsoft.AspNetCore.Mvc;
using Suscripcion.Application.DTOs;
using Suscripcion.Application.Users;

namespace Suscripcion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly RegisterUserHandler _handler;

    public UserController(RegisterUserHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Datos de registro incluyendo nombre y email.</param>
    /// <returns>Detalles del usuario con userId generado o BadRequest si falla la validación.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponseDto>> Register([FromBody] RegisterUserRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var result = await _handler.Handle(request);
        
        if (result.UserId == 0)
            return BadRequest(result);
            
        return Ok(result);
    }
}
