using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Models;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Contexts;
using Microsoft.AspNetCore.Http;

namespace Eventos_ProjetoFinal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventosService _eventosService;
        private readonly BdDbContext _context;
        public HomeController(IEventosService eventosService, BdDbContext context)
        {
            _eventosService = eventosService;
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            var eventosAtivos = await _eventosService.ListarAtivos();
            return View(eventosAtivos);
        }

        [HttpPost]
        public async Task<IActionResult> Inscrever(int eventoId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Aluno")
            {
                return RedirectToAction("Index", "Login");
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId")!);
            var isAluno = await _context.Aluno.FindAsync(usuarioId);
            if (isAluno == null)
            {
                TempData["Erro"] = "Apenas alunos podem se inscrever em eventos.";
                return RedirectToAction("Index");
            }

            var inscricao = new Inscricao
            {
                AlunoID = usuarioId,
                EventoID = eventoId,
                DataInscricao = DateOnly.FromDateTime(DateTime.Now)
            };
            try
            {
                await _context.Inscricao.AddAsync(inscricao);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Inscrição realizada com sucesso!";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao realizar inscrição.";
            }
            return RedirectToAction("Index", "Home");
        }
    }
}