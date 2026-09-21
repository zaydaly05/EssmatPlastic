using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API;

public static class NeonSchemaInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();
    }
}
