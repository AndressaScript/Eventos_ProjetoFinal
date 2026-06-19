using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;

namespace Eventos_ProjetoFinal.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public UsuarioService(IUsuarioRepository usuarioRepository) { _usuarioRepository = usuarioRepository; }

        public async Task<LoginResult> Autenticar(string email, string senha)
        {
            var usuario = await _usuarioRepository.BuscarPorEmail(email);
            if (usuario == null)
            {
                return new LoginResult { Status = LoginResultStatus.EmailInvalido };
            }

            if (usuario.Senha != senha)
            {
                return new LoginResult { Status = LoginResultStatus.SenhaIncorreta };
            }

            var role = await _usuarioRepository.ObterRoleUsuario(usuario.UserID);
            if (role == null)
            {
                return new LoginResult { Status = LoginResultStatus.PerfilNaoEncontrado };
            }

            return new LoginResult { Status = LoginResultStatus.Sucesso, Usuario = usuario, Role = role };
        }

        public async Task<string?> ObterRoleUsuario(int userId)
        {
            return await _usuarioRepository.ObterRoleUsuario(userId);
        }
    }
}