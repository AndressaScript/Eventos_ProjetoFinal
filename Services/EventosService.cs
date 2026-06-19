using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;

namespace Eventos_ProjetoFinal.Services
{
    public class EventosService : IEventosService
    {
        private readonly IEventosRepository _repo;

        public EventosService(IEventosRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Eventos>> ListarTodos()
        {
            return await _repo.ListarTodos();
        }

        public async Task<IEnumerable<Eventos>> ListarAtivos()
        {
            return await _repo.ListarAtivos();
        }

        public async Task<IEnumerable<Eventos>> ListarPorAdmin(int adminId)
        {
            return await _repo.ListarPorAdmin(adminId);
        }

        public async Task<Eventos?> BuscarPorId(int id)
        {
            return await _repo.BuscarPorId(id);
        }

        public async Task Criar(Eventos evento)
        {
            await _repo.Criar(evento);
        }

        public async Task Atualizar(Eventos evento)
        {
            await _repo.Atualizar(evento);
        }

        public async Task Excluir(int id)
        {
            await _repo.Excluir(id);
        }
    }
}