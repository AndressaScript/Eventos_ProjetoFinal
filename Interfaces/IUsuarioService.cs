using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Models;

namespace Eventos_ProjetoFinal.Interfaces
{
    public interface IUsuarioService
    {
        Task<LoginResult> Autenticar(string email, string senha);
        Task<string?> ObterRoleUsuario(int userId);
    }
}