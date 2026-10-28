namespace WebTemplate.Models;

public class IspitContext : DbContext
{
    // DbSet kolekcije!

    public IspitContext(DbContextOptions options) : base(options)
    {
        
    }
}
