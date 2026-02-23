using Case3.Modern.Api.Models;

namespace Case3.Modern.Api.Services;

/// <summary>
/// Implementacao de exemplo do servico de usuarios para demonstrar o fluxo corrigido do case 3.
/// </summary>
public sealed class UserService : IUserService
{
    /// <summary>
    /// Simula a consulta de usuario respeitando o contrato do endpoint corrigido.
    /// Retorna null para entradas invalidas e para o id 404, permitindo ao controller responder 404.
    /// </summary>
    public Task<UserDto?> GetUserAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0 || id == 404)
        {
            return Task.FromResult<UserDto?>(null);
        }

        var user = new UserDto
        {
            Id = id,
            Name = "Modern User"
        };

        return Task.FromResult<UserDto?>(user);
    }
}
