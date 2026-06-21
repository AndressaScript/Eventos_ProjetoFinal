using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Models;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        
        // Página Inicial: Carrossel com os próximos 6 eventos
        public async Task<IActionResult> Index()
        {
            var eventosAtivos = await _eventosService.ListarAtivos();
            
            var role = HttpContext.Session.GetString("Role");
            if (role == "Aluno")
            {
                if (int.TryParse(HttpContext.Session.GetString("UsuarioId"), out int alunoId))
                {
                    var inscricoes = await _context.Inscricao
                        .Where(i => i.AlunoID == alunoId)
                        .Select(i => i.EventoID)
                        .ToListAsync();
                    ViewBag.InscritosIds = inscricoes;
                }
            }

            // Filtra por próximos eventos (futuros ou hoje), ordena por data e pega no máximo 6
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var proximosEventos = eventosAtivos
                .Where(e => e.DataEvento >= hoje)
                .OrderBy(e => e.DataEvento)
                .ThenBy(e => e.HorarioEvento.TimeOfDay)
                .Take(6)
                .ToList();
            
            return View(proximosEventos);
        }

        // Página de Listagem Geral: Exibe todos os eventos ativos em Grid
        [HttpGet]
        public async Task<IActionResult> Todos()
        {
            var eventosAtivos = await _eventosService.ListarAtivos();
            
            var role = HttpContext.Session.GetString("Role");
            if (role == "Aluno")
            {
                if (int.TryParse(HttpContext.Session.GetString("UsuarioId"), out int alunoId))
                {
                    var inscricoes = await _context.Inscricao
                        .Where(i => i.AlunoID == alunoId)
                        .Select(i => i.EventoID)
                        .ToListAsync();
                    ViewBag.InscritosIds = inscricoes;
                }
            }

            // Ordena todos os eventos ativos por data
            var ordenados = eventosAtivos
                .OrderBy(e => e.DataEvento)
                .ThenBy(e => e.HorarioEvento.TimeOfDay)
                .ToList();
                
            return View(ordenados);
        }

        // Método tradicional de inscrição
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

        // NOVO: Endpoint AJAX para inscrição fluida no Modal de Detalhes
        [HttpPost]
        public async Task<IActionResult> InscreverAjax(int eventoId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Aluno")
            {
                return Json(new { success = false, redirectUrl = Url.Action("Index", "Login") });
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId")!);
            var isAluno = await _context.Aluno.FindAsync(usuarioId);
            if (isAluno == null)
            {
                return Json(new { success = false, message = "Apenas alunos podem se inscrever em eventos." });
            }

            // Evita duplicidade de inscrição
            var existente = await _context.Inscricao
                .FirstOrDefaultAsync(i => i.AlunoID == usuarioId && i.EventoID == eventoId);
                
            if (existente != null)
            {
                return Json(new { success = true, message = "Você já está inscrito neste evento." });
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
                return Json(new { success = true, message = "Inscrição realizada com sucesso!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Erro ao processar a inscrição no banco de dados." });
            }
        }
    }
}