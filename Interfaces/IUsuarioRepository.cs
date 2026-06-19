using Eventos_ProjetoFinal.Models;

namespace Eventos_ProjetoFinal.Interfaces
{
    public interface IUsuarioRepository
    {
        Task <Usuario?> BuscarPorEmail (string email);
        Task <string?> ObterRoleUsuario (int userId);
    }
}