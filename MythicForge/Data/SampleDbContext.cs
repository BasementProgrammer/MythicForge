using System.Data.Entity;
using MythicForge.Models;

namespace MythicForge.Data
{
    /// <summary>
    /// Entity Framework database context for the mystical creature store. The
    /// connection string named "SampleDbContext" in Web.config points at a local
    /// LocalDB .mdf file under App_Data, keeping the sample self-contained.
    /// </summary>
    public class SampleDbContext : DbContext
    {
        public SampleDbContext()
            : base("name=SampleDbContext")
        {
        }

        public DbSet<CreatureType> CreatureTypes { get; set; }
        public DbSet<OptionCategory> OptionCategories { get; set; }
        public DbSet<CreatureOption> CreatureOptions { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Emails are unique lookups.
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(256);

            // A creature type owns its option categories, which own their options.
            modelBuilder.Entity<OptionCategory>()
                .HasRequired(c => c.CreatureType)
                .WithMany(t => t.OptionCategories)
                .HasForeignKey(c => c.CreatureTypeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CreatureOption>()
                .HasRequired(o => o.OptionCategory)
                .WithMany(c => c.Options)
                .HasForeignKey(o => o.OptionCategoryId)
                .WillCascadeOnDelete(true);

            // An order owns its line items.
            modelBuilder.Entity<OrderItem>()
                .HasRequired(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
