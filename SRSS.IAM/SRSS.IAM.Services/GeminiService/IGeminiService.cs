using System.Threading.Tasks;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.GeminiService
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt);
        Task<T> GenerateStructuredContentAsync<T>(string prompt);
    }
}