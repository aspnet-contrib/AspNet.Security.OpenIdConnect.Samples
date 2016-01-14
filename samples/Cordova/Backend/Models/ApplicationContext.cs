using Microsoft.Data.Entity;

namespace Backend.Models {
    public class ApplicationContext : DbContext {
        public DbSet<Application> Applications { get; set; }
    }
}