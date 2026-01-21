using System.Threading.Tasks;
using MegaMall.Models;

namespace MegaMall.Interfaces
{
    public interface IAIService
    {
        Task<string?> GenerateTextAsync(string prompt, int maxTokens = 512, double temperature = 0.2);
        Task<AiEmailResult> GenerateEmailAsync(string templateKey, object variables);
    }
}
