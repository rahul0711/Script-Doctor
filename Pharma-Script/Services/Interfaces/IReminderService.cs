using System.Threading.Tasks;

namespace Pharma_Script.Services.Interfaces
{
    public interface IReminderService
    {
        Task<int> GenerateDailyRemindersAsync();
    }
}
