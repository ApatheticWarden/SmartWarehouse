using Microsoft.EntityFrameworkCore;
using SmartWarehouse.Models;

namespace SmartWarehouse.Database {
    public class SmartWarehouseContext: DbContext {
        // options are in appsettings.json
        public SmartWarehouseContext(DbContextOptions<SmartWarehouseContext> options) : base(options) { }
        // This is needed by DI -> No hardcoded cons

        // Add tables here
        public DbSet<Package> Packages { get; set; }  
        public DbSet<User> Users { get; set; }
        public DbSet<Rack> Racks { get; set; }
        public DbSet<Robot> Robots { get; set; }
        public DbSet<WarehouseTask> WarehouseTasks { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Rack)
                .WithMany(r => r.Packages)
                .HasForeignKey(p => p.RackId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WarehouseTask>()
                .HasOne(t => t.Rack)
                .WithMany(r => r.Tasks)
                .HasForeignKey(t => t.RackId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarehouseTask>()
                .HasOne(t => t.AssignedRobot)
                .WithMany(r => r.Tasks)
                .HasForeignKey(t => t.AssignedRobotId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
