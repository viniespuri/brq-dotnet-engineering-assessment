using Case3.Modern.Api.Models;

namespace Case3.Modern.Api.Services;

/// <summary>
/// Contrato de acesso a usuario no case 3 moderno, desacoplando controller da implementacao concreta.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Busca um usuario por id de forma assincrona, permitindo cancelamento da requisicao.
    /// Retorna null quando o usuario nao existe.
    /// </summary>
    Task<UserDto?> GetUserAsync(int id, CancellationToken cancellationToken);
}
