using Google.Cloud.Vision.V1;
using Google.Apis.Auth.OAuth2;
using Eventos_ProjetoFinal.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Eventos_ProjetoFinal.Services
{
    public class ModeracaoService : IModeracaoService
    {
        public async Task<bool> EConteudoSeguro(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0) return true;

            var credenciaisPath = Path.Combine(Directory.GetCurrentDirectory(), "google-credentials.json");
            ImageAnnotatorClient client;

            // 1. Inicializa o cliente usando as credenciais portáteis se o arquivo existir
            if (File.Exists(credenciaisPath))
            {
                var builder = new ImageAnnotatorClientBuilder
                {
                    CredentialsPath = credenciaisPath
                };
                client = await builder.BuildAsync();
            }
            else
            {
                client = await ImageAnnotatorClient.CreateAsync();
            }

            // 2. Converte o arquivo recebido para a classe Image do Google Vision
            using var stream = arquivo.OpenReadStream();
            // Usamos o namespace completo para evitar conflitos com System.Drawing ou outras bibliotecas
            var image = Google.Cloud.Vision.V1.Image.FromStream(stream); 

            // 3. Executa a detecção de Safe Search (MÉTODO CORRETO E RETORNO DIRETO)
            SafeSearchAnnotation annotation = await client.DetectSafeSearchAsync(image);

            // 4. Se qualquer um dos níveis for "Likely" ou "VeryLikely", bloqueamos
            if (annotation.Adult >= Likelihood.Likely || 
                annotation.Violence >= Likelihood.Likely ||
                annotation.Racy >= Likelihood.Likely ||
                annotation.Medical >= Likelihood.Likely ||
                annotation.Spoof >= Likelihood.Likely)
            {
                return false; // Conteúdo impróprio detectado
            }

            return true; // Conteúdo seguro
        }
    }
}