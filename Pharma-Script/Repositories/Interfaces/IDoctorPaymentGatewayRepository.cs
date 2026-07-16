using Pharma_Script.Models;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IDoctorPaymentGatewayRepository : IRepository<DoctorPaymentGateway>
    {
        Task<DoctorPaymentGateway?> GetByDoctorIdAsync(int doctorId, string provider = "Razorpay");
        Task<int> UpsertAsync(DoctorPaymentGateway entity);
    }
}
