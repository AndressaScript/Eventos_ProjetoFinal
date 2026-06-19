
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
            var evento = await BuscarPorId(id);
            if (evento != null)
            {
                _context.Eventos.Remove(evento);
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