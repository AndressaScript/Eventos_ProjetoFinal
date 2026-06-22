using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace Eventos_ProjetoFinal.Interfaces
{
    public interface IModeracaoService
    {
        Task<bool> EConteudoSeguro(IFormFile arquivo);
    }
}