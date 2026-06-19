using System;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Eventos_ProjetoFinal.Controllers
{
    public class EventosController : Controller
    {
        // Filtro de Autorização: Permite Aluno apenas na Galeria, e Admin nas demais telas
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"]?.ToString();
            var role = HttpContext.Session.GetString("Role");
            if (action == "Galeria")
            {
                // Galeria é acessível tanto por Admin quanto por Aluno
                if (role != "Admin" && role != "Aluno")
                {
                    context.Result = new RedirectToActionResult("Index", "Login", null);
                }
            }
            else
            {
                // Todas as outras ações de gestão exigem perfil de Admin
                if (role != "Admin")
                {
                    context.Result = new RedirectToActionResult("Index", "Login", null);
                }
            }
            base.OnActionExecuting(context);
        }
        private readonly IEventosService _service;
        public EventosController(IEventosService service)
        {
            _service = service;
        }
        // Método auxiliar para carregar as estatísticas da sidebar dinamicamente nas views de formulário
        private async Task CarregarEstatisticasSidebar()
        {
            try
            {
                var eventos = await _service.ListarTodos();
                ViewBag.EventosAtivos = eventos.Count(e => e.StatusEvento == "Ativo");
                ViewBag.Palestrantes = eventos.Select(e => e.NomePalestrante).Distinct().Count();
            }
            catch (Exception)
            {
                ViewBag.EventosAtivos = 0;
                ViewBag.Palestrantes = 0;
            }
        }
        public async Task<IActionResult> Index()
        {
            var listaDeEventos = await _service.ListarTodos();
            return View(listaDeEventos);
        }
        [HttpGet]
        public async Task<IActionResult> Cadastrar()
        {
            await CarregarEstatisticasSidebar();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Cadastrar(Eventos novoEvento)
        {
            ModelState.Remove("AdminID");
            if (ModelState.IsValid)
            {
                if (int.TryParse(HttpContext.Session.GetString("UsuarioId"), out int adminId))
                {
                    novoEvento.AdminID = adminId;
                    await _service.Criar(novoEvento);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Admin ID não encontrado na sessão.");
            }
            
            await CarregarEstatisticasSidebar();
            return View(novoEvento);
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var evento = await _service.BuscarPorId(id);
            if (evento == null) return NotFound();
            
            await CarregarEstatisticasSidebar();
            return View(evento);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(Eventos evento)
        {
             ModelState.Remove("AdminID");
             if (ModelState.IsValid)
             {
                 if (int.TryParse(HttpContext.Session.GetString("UsuarioId"), out int adminId))
                 {
                     evento.AdminID = adminId;
                     await _service.Atualizar(evento);
                     return RedirectToAction("Index");
                 }
                 ModelState.AddModelError("", "Admin ID não encontrado na sessão.");
             }
             
             await CarregarEstatisticasSidebar();
             return View(evento);
        }
        public async Task<IActionResult> Galeria()
{
    var listaDeEventos = await _service.ListarTodos();
    // Filtra para exibir apenas os eventos que já ocorreram (data anterior a hoje)
    var eventosPassados = listaDeEventos.Where(e => e.DataEvento < DateOnly.FromDateTime(DateTime.Today)).ToList();
    return View(eventosPassados);
}
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            await _service.Excluir(id);
            return RedirectToAction("Index");
        }
    }
}