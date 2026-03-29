using System.Threading.Tasks;

namespace SRSS.IAM.Services.GeminiService
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt);
        Task<T> GenerateStructuredContentAsync<T>(string prompt);
    }
}