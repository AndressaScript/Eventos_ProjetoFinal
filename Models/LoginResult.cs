using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventos_ProjetoFinal.Models
{

    public enum LoginResultStatus
    {
        Sucesso,
        EmailInvalido,
        SenhaIncorreta,
        PerfilNaoEncontrado
    }
    public class LoginResult
    {
        public LoginResultStatus Status { get; set; }

        public Usuario? Usuario { get; set; }

        public string? Role { get; set; }
    }
}