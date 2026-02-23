using Case3.Modern.Api.Models;
using Case3.Modern.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Case3.Modern.Api.Controllers;

[ApiController]
[Route("v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Injeta o servico para reduzir acoplamento e facilitar testes, substituindo a criacao direta com new.
    /// </summary>
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Ajusta o contrato para GET /v1/users/{id}, valida id invalido (400), retorna 404 quando nao encontrado e 200 no sucesso.
    /// Mantem o fluxo assincrono com CancellationToken como parte da versao moderna do case 3.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUserAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("id must be greater than zero.");
        }

        var user = await _userService.GetUserAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
