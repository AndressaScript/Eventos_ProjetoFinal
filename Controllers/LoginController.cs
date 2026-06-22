using Microsoft.AspNetCore.Mvc;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Eventos_ProjetoFinal.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public LoginController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logar(string email, string senha)
        {
            var result = await _usuarioService.Autenticar(email, senha);

            if (result.Status == LoginResultStatus.Sucesso && result.Usuario != null)
            {
                HttpContext.Session.SetString("UsuarioId", result.Usuario.UserID.ToString());
                HttpContext.Session.SetString("Role", result.Role ?? "");
                HttpContext.Session.SetString("Email", result.Usuario.Email);

                // CORREÇÃO: Admin agora é redirecionado para a Home (Dashboard Geral) ao invés da tabela crua de eventos
                if (result.Role == "Admin")
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (result.Role == "Aluno")
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Erro = result.Status switch
            {
                LoginResultStatus.EmailInvalido => "Email não cadastrado.",
                LoginResultStatus.SenhaIncorreta => "Senha incorreta.",
                LoginResultStatus.PerfilNaoEncontrado => "Perfil de usuário não encontrado.",
                _ => "Erro ao realizar login."
            };

            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}