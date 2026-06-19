using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Models;

namespace Eventos_ProjetoFinal.Interfaces
{
    public interface IEventosRepository
    {
        Task<IEnumerable<Eventos>> ListarAtivos();
        Task<IEnumerable<Eventos>> ListarTodos();
        Task<IEnumerable<Eventos>> ListarPorAdmin(int adminId);
        Task <Eventos?> BuscarPorId (int id);
        Task Criar (Eventos evento);
        Task Atualizar(Eventos evento);

        Task Excluir (int id);

    }
}