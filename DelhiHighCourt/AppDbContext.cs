using Microsoft.EntityFrameworkCore;

namespace DelhiHighCourt;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<caseDetail> caseDetails{ get; set; }
}
