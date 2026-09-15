using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineMonitor.Infrastructure.Persistence;

public sealed class UserRepository(MedicineMonitorDbContext db) : IUserRepository
{
    public Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

    public async Task<User> UpsertAsync(User user, CancellationToken cancellationToken)
    {
        var existing = await db.Users.SingleOrDefaultAsync(x => x.FirebaseUid == user.FirebaseUid, cancellationToken);
        if (existing is null)
            await db.Users.AddAsync(user, cancellationToken);

        return existing ?? user;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
