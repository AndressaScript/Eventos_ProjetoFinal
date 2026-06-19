using Eventos_ProjetoFinal.Models;

namespace Eventos_ProjetoFinal.Interfaces
{
    public interface IEventosService
    {
        Task<IEnumerable<Eventos>> ListarAtivos();
        Task<IEnumerable<Eventos>> ListarTodos();
        Task<IEnumerable<Eventos>> ListarPorAdmin(int adminID);
        Task <Eventos?> BuscarPorId (int id);
        Task Criar (Eventos evento);
        Task Atualizar(Eventos evento);

        Task Excluir (int id);
    }
}