using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;
using Eventos_ProjetoFinal.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Eventos_ProjetoFinal.Controllers
{
    public class EventosController : Controller
    {
        private readonly IEventosService _service;
        private readonly BdDbContext _context;
        private readonly IModeracaoService _moderacao; // Injeção da IA de Moderação

        public EventosController(IEventosService service, BdDbContext context, IModeracaoService moderacao)
        {
            _service = service;
            _context = context;
            _moderacao = moderacao;
        }

        // Filtro de Autorização
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"]?.ToString();
            var role = HttpContext.Session.GetString("Role");
           
            if (action == "Galeria" || action == "UploadFotos" || action == "Visualizar")
            {
                if (role != "Admin" && role != "Aluno")
                {
                    context.Result = new RedirectToActionResult("Index", "Login", null);
                }
            }
            else
            {
                if (role != "Admin")
                {
                    context.Result = new RedirectToActionResult("Index", "Login", null);
                }
            }
            base.OnActionExecuting(context);
        }

        private async Task CarregarEstatisticasSidebar()
        {
            try
            {
                var list = await _service.ListarTodos();
                ViewBag.EventosAtivos = list.Count(e => e.StatusEvento == "Ativo");
                ViewBag.Palestrantes = list.Select(e => e.NomePalestrante).Distinct().Count();
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
        public async Task<IActionResult> Cadastrar(Eventos novoEvento, string horaInput, IFormFile? imagemArquivo)
        {
            ModelState.Remove("AdminID");
            ModelState.Remove("HorarioEvento");

            // Mescla a Data (DateOnly) com o Horário (string)
            if (TimeSpan.TryParse(horaInput, out var time))
            {
                novoEvento.HorarioEvento = new DateTime(
                    novoEvento.DataEvento.Year,
                    novoEvento.DataEvento.Month,
                    novoEvento.DataEvento.Day,
                    time.Hours,
                    time.Minutes,
                    0
                );
            }
            else
            {
                ModelState.AddModelError("HorarioEvento", "O horário selecionado é inválido.");
            }

            if (ModelState.IsValid)
            {
                if (int.TryParse(HttpContext.Session.GetString("UsuarioId"), out int adminId))
                {
                    novoEvento.AdminID = adminId;

                    // Upload do Banner se fornecido
                    if (imagemArquivo != null && imagemArquivo.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imagemArquivo.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagemArquivo.CopyToAsync(stream);
                        }

                        novoEvento.ImagemUrl = "/uploads/" + uniqueFileName;
                    }

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
        public async Task<IActionResult> Editar(Eventos evento, string horaInput, IFormFile? imagemArquivo)
        {
             ModelState.Remove("AdminID");
             ModelState.Remove("HorarioEvento");

             // Mescla a Data (DateOnly) com o Horário (string)
             if (TimeSpan.TryParse(horaInput, out var time))
             {
                 evento.HorarioEvento = new DateTime(
                     evento.DataEvento.Year,
                     evento.DataEvento.Month,
                     evento.DataEvento.Day,
                     time.Hours,
                     time.Minutes,
                     0
                 );
             }
             else
             {
                 ModelState.AddModelError("HorarioEvento", "O horário selecionado é inválido.");
             }

             if (ModelState.IsValid)
             {
                  if (int.TryParse(HttpContext.Session.GetString("UsuarioId"), out int adminId))
                  {
                      evento.AdminID = adminId;

                      // Upload do Banner se fornecido
                      if (imagemArquivo != null && imagemArquivo.Length > 0)
                      {
                          var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                          if (!Directory.Exists(uploadsFolder))
                          {
                              Directory.CreateDirectory(uploadsFolder);
                          }

                          var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imagemArquivo.FileName);
                          var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                          using (var stream = new FileStream(filePath, FileMode.Create))
                          {
                              await imagemArquivo.CopyToAsync(stream);
                          }

                          evento.ImagemUrl = "/uploads/" + uniqueFileName;
                      }
                      else
                      {
                          // Preserva a ImagemUrl antiga se não foi enviada uma nova
                          var eventoOriginal = await _context.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.EventoID == evento.EventoID);
                          if (eventoOriginal != null)
                          {
                              evento.ImagemUrl = eventoOriginal.ImagemUrl;
                          }
                      }

                      await _service.Atualizar(evento);
                      return RedirectToAction("Index");
                  }
                  ModelState.AddModelError("", "Admin ID não encontrado na sessão.");
             }
             await CarregarEstatisticasSidebar();
             return View(evento);
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            await _service.Excluir(id);
            return RedirectToAction("Index");
        }

        // GET: Exibe a Galeria de Eventos (Mais leve, não carrega fotos aqui!)
        public async Task<IActionResult> Galeria()
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var listaDeEventos = await _service.ListarTodos();
           
            var eventosPassados = listaDeEventos
                .Where(e => e.DataEvento <= hoje)
                .OrderByDescending(e => e.DataEvento)
                .ToList();

            var role = HttpContext.Session.GetString("Role");
            var userIdStr = HttpContext.Session.GetString("UsuarioId");

            List<Eventos> eventosDisponiveisParaUpload = new List<Eventos>();

            if (role == "Admin")
            {
                eventosDisponiveisParaUpload = eventosPassados;
            }
            else if (role == "Aluno" && int.TryParse(userIdStr, out int alunoId))
            {
                var meusEventosIds = await _context.Inscricao
                    .Where(i => i.AlunoID == alunoId)
                    .Select(i => i.EventoID)
                    .ToListAsync();

                eventosDisponiveisParaUpload = eventosPassados
                    .Where(e => meusEventosIds.Contains(e.EventoID))
                    .ToList();
            }

            ViewBag.EventosDisponiveis = eventosDisponiveisParaUpload;

            // Busca as fotos apenas para contar a quantidade em cada álbum de forma leve
            var contagemFotos = await _context.Galeria
                .GroupBy(g => g.EventoID)
                .Select(g => new { EventoID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EventoID, x => x.Count);

            ViewBag.ContagemFotos = contagemFotos;

            // Estatística rápida de álbuns criados
            ViewBag.QtdAlbuns = contagemFotos.Count(x => x.Value > 0);

            return View(eventosPassados);
        }

        // GET: Nova Página Dedicada para ver as fotos de um Evento Específico
        [HttpGet]
        public async Task<IActionResult> Visualizar(int id)
        {
            var evento = await _service.BuscarPorId(id);
            if (evento == null) return NotFound();

            var fotos = await _context.Galeria
                .Include(g => g.Usuario)
                .Where(g => g.EventoID == id)
                .OrderByDescending(g => g.DataUpload)
                .ToListAsync();

            ViewBag.Evento = evento;
            return View(fotos);
        }

        // POST: Faz o Upload de múltiplas fotos (COM MODERAÇÃO DA IA ATIVA)
        [HttpPost]
        public async Task<IActionResult> UploadFotos(int eventoId, List<IFormFile> imagens, List<string> legendas)
        {
            var role = HttpContext.Session.GetString("Role");
            var userIdStr = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(role) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Index", "Login");
            }

            // Regra de Aluno: Precisa ter participado e as imagens passam pela moderação por IA
            if (role == "Aluno")
            {
                var participou = await _context.Inscricao
                    .AnyAsync(i => i.AlunoID == userId && i.EventoID == eventoId);

                if (!participou)
                {
                    TempData["Erro"] = "Você só pode enviar fotos de eventos nos quais participou.";
                    return RedirectToAction("Galeria");
                }

                // 🌟 MODERAÇÃO EM LOOP COM INTELIGÊNCIA ARTIFICIAL (GOOGLE VISION API)
                if (imagens != null && imagens.Any())
                {
                    foreach (var imagem in imagens)
                    {
                        bool seguro = await _moderacao.EConteudoSeguro(imagem);
                        if (!seguro)
                        {
                            TempData["Erro"] = "Upload cancelado: Uma ou mais imagens violam as diretrizes de conteúdo (conteúdo impróprio ou ofensivo).";
                            return RedirectToAction("Galeria");
                        }
                    }
                }
            }

            if (imagens == null || !imagens.Any())
            {
                TempData["Erro"] = "Nenhuma imagem selecionada para upload.";
                return RedirectToAction("Galeria");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "galeria");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            int fotosSalvas = 0;
            for (int i = 0; i < imagens.Count; i++)
            {
                var file = imagens[i];
                if (file.Length > 0)
                {
                    if (file.Length > 5 * 1024 * 1024) continue;

                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;

                    var uniqueFileName = Guid.NewGuid().ToString() + ext;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string? legenda = null;
                    if (legendas != null && legendas.Count > i)
                    {
                        legenda = legendas[i];
                    }

                    var novaFoto = new Galeria
                    {
                        EventoID = eventoId,
                        UserID = userId,
                        CaminhoImagem = "/uploads/galeria/" + uniqueFileName,
                        Legenda = string.IsNullOrWhiteSpace(legenda) ? null : legenda,
                        DataUpload = DateTime.Now
                    };

                    await _context.Galeria.AddAsync(novaFoto);
                    fotosSalvas++;
                }
            }

            if (fotosSalvas > 0)
            {
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = $"{fotosSalvas} foto(s) publicada(s) com sucesso na galeria!";
            }
            else
            {
                TempData["Erro"] = "Nenhuma foto pôde ser salva (tamanho excedido ou formato inválido).";
            }

            return RedirectToAction("Galeria");
        }
    }
}