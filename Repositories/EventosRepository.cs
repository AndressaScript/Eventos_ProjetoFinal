using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Contexts;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos_ProjetoFinal.Repositories
{
    public class EventosRepository : IEventosRepository
    {
        private readonly BdDbContext _context;

        public EventosRepository(BdDbContext context)
        {
            _context = context;
        }
        
        public async Task Atualizar(Eventos evento)
        {
            _context.Eventos.Update(evento);
            await _context.SaveChangesAsync();
        }

        public async Task<Eventos?> BuscarPorId(int id)
        {
            return await _context.Eventos.FindAsync(id);
        }

        public async Task Criar(Eventos evento)
        {
            await _context.Eventos.AddAsync(evento);
            await _context.SaveChangesAsync();
        }

        public async Task Excluir(int id)
        {
            // 1. Busca e remove todas as avaliações (comentários) associadas a este evento
            var avaliacoes = await _context.Avaliacao.Where(a => a.EventoID == id).ToListAsync();
            if (avaliacoes.Any())
            {
                _context.Avaliacao.RemoveRange(avaliacoes);
            }

            // 2. Busca e remove todas as inscrições associadas a este evento
            var inscricoes = await _context.Inscricao.Where(i => i.EventoID == id).ToListAsync();
            if (inscricoes.Any())
            {
                _context.Inscricao.RemoveRange(inscricoes);
            }

            // ETAPA 1: Salva as remoções dos registros filhos primeiro.
            // Isso limpa fisicamente as dependências no SQL Server antes de qualquer outra ação.
            await _context.SaveChangesAsync();

            // 3. Busca e remove o evento principal
            var evento = await BuscarPorId(id);
            if (evento != null)
            {
                _context.Eventos.Remove(evento);
                
                // ETAPA 2: Com o caminho limpo, apaga o evento.
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Eventos>> ListarAtivos()
        {
            return await _context.Eventos.Where(e => e.StatusEvento == "Ativo").ToListAsync();
        }

        public async Task<IEnumerable<Eventos>> ListarPorAdmin(int adminId)
        {
            return await _context.Eventos.Where(e => e.AdminID == adminId).ToListAsync();
        }

        public async Task<IEnumerable<Eventos>> ListarTodos()
        {
            return await _context.Eventos.ToListAsync();
        }
    }
}