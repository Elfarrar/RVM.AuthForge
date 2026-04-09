using RVM.AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVM.AuthForge.Tests.Helpers;

public static class TestDbContext
{
    public static AuthForgeDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AuthForgeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AuthForgeDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
