using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using System;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Contexts;
using Eventos_ProjetoFinal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Eventos_ProjetoFinal.Controllers
{
    public class ChatController : Controller
    {
        private readonly string _apiKey;
        private readonly BdDbContext _context;

        public ChatController(IConfiguration configuration, BdDbContext context)
        {
            _apiKey = configuration["GeminiApiKey"];
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensagem([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Mensagem))
            {
                return BadRequest(new { resposta = "A mensagem não pode ser vazia." });
            }

            try
            {
                if (string.IsNullOrEmpty(_apiKey) || _apiKey.Contains("COLE_SUA_CHAVE"))
                {
                    return Json(new { resposta = "⚠️ O assistente de chat está indisponível no momento (chave de API do Gemini não configurada no servidor)." });
                }

                // ==========================================================================
                // ETAPA 1: BUSCA DE DADOS EM TEMPO REAL NO BANCO DE DADOS (RAG)
                // ==========================================================================
                var agora = DateTime.Now; // Hora local do servidor da escola/casa

                // 1. Busca estritamente o próximo evento ativo que AINDA VAI ACONTECER (futuro)
                var proximoEvento = await _context.Eventos
                    .Where(e => e.HorarioEvento > agora && e.StatusEvento == "Ativo")
                    .OrderBy(e => e.HorarioEvento)
                    .FirstOrDefaultAsync();

                // 2. Conta o total de eventos ativos futuros disponíveis para inscrição
                int totalEventosDisponiveis = await _context.Eventos
                    .CountAsync(e => e.HorarioEvento > agora && e.StatusEvento == "Ativo");

                // 3. Busca dados do Aluno Logado na Sessão
                int totalInscricoes = 0;
                bool isAlunoLogado = false;
                var role = HttpContext.Session.GetString("Role");
                var userIdStr = HttpContext.Session.GetString("UsuarioId");

                if (role == "Aluno" && int.TryParse(userIdStr, out int alunoId))
                {
                    isAlunoLogado = true;
                    totalInscricoes = await _context.Inscricao.CountAsync(i => i.AlunoID == alunoId);
                }

                // ==========================================================================
                // ETAPA 2: MONTAGEM DO PROMPT DINÂMICO
                // ==========================================================================
               
                // Formata as informações do próximo evento (garantidamente futuro)
                string infoProximoEvento = proximoEvento != null
                    ? $"- PROXIMO EVENTO AGENDADO (AINDA VAI OCORRER): '{proximoEvento.NomeEvento}'.\n" +
                      $"  * DATA E HORA DE INÍCIO: {proximoEvento.HorarioEvento.ToString("dd/MM/yyyy 'às' HH:mm")}h.\n" +
                      $"  * PALESTRANTE CONVIDADO: {proximoEvento.NomePalestrante}.\n" +
                      $"  * LOCAL DO EVENTO: {proximoEvento.LocalEvento}.\n" +
                      $"  * CARGA HORÁRIA (DURAÇÃO EM HORAS): {proximoEvento.CargaHorariaEvento}.\n" +
                      $"  * CATEGORIA/TIPO: {proximoEvento.TipoEvento}."
                    : "- PROXIMO EVENTO AGENDADO: Não há nenhum evento futuro agendado no momento.";

                // Formata o total de eventos disponíveis
                string infoGeralBanco = $"- TOTAL DE EVENTOS DISPONÍVEIS: Existem atualmente exatamente {totalEventosDisponiveis} evento(s) futuro(s) ativo(s) com inscrições abertas no portal.";

                // Formata as inscrições do aluno
                string infoAluno = isAlunoLogado
                    ? $"- DADOS DO ALUNO LOGADO: O usuário conversando com você é um Aluno logado. Ele já se inscreveu/participou de exatamente {totalInscricoes} evento(s) no total neste portal."
                    : "- DADOS DO ALUNO: O usuário não está logado. Se ele perguntar quantos eventos participou, diga simpaticamente para fazer login no menu superior.";

                // 4. Inicialização da IA do Gemini 2.5 Flash
                var googleAI = new GoogleAI(apiKey: _apiKey);
                var model = googleAI.GenerativeModel(Model.Gemini25Flash);

                // Prompt System unificando as novas regras de dados dinâmicos do banco
                string dadosDoEvento = $@"
                Você é o assistente virtual simpático e oficial do 'Centro de Eventos Senai Paulo Skaf 2026'.
                Use estritamente as informações dinâmicas obtidas em tempo real do nosso banco de dados abaixo para responder:
               
                DADOS EM TEMPO REAL DO BANCO DE DADOS (SQL SERVER):
                {infoProximoEvento}
                {infoGeralBanco}
                {infoAluno}
               
                INFORMAÇÕES GERAIS DE CONTATO:
                - E-MAIL DE SUPORTE: suporte@eventossenai.com.br.
                - LOCAL DO CENTRO: Rua Niterói, 180 - Centro, São Caetano do Sul - SP.
               
                REGRAS DE COMPORTAMENTO (OBRIGATÓRIO):
                1. Seja sempre cordial, educado, curto e direto nas respostas (máximo 3 linhas ou parágrafos compactos).
                2. Se o usuário perguntar qual a data do próximo evento, palestrante, carga horária ou quantos eventos existem, use os dados em tempo real acima.
                3. Se o usuário perguntar algo que NÃO CONSTA nas informações dinâmicas ou gerais acima, responda exatamente: 'Desculpe, não tenho essa informação no momento. Você pode entrar em contato com a nossa equipe pelo e-mail suporte@eventossenai.com.br'.
                4. Nunca invente ou assuma qualquer dado que não esteja explicitamente listado nos dados em tempo real acima.
               
                ";

                string promptFinal = $"{dadosDoEvento}\n\nPergunta do Usuário: {request.Mensagem}\nResposta:";

                // 5. Chamada assíncrona ao Gemini API
                var response = await model.GenerateContent(promptFinal);
               
                return Json(new { resposta = response.Text });
            }
            catch (Exception ex)
            {
                return Json(new { resposta = $"❌ Erro ao processar a resposta: {ex.Message}" });
            }
        }
    }

    public class ChatRequest
    {
        public string Mensagem { get; set; } = null!;
    }
}