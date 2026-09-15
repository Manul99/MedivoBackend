using MedicineMonitor.Domain.Entities;

namespace MedicineMonitor.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken);
    Task<User> UpsertAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
