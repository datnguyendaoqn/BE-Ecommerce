// Ecommerce.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using BackendEcommerce.Models;

namespace BackendEcommerce.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) {}

        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<AddressBook> AddressBook { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // map table names to match Oracle schema (uppercase)
            builder.Entity<Account>().ToTable("ACCOUNTS");
            builder.Entity<User>().ToTable("USERS");
            builder.Entity<Shop>().ToTable("SHOPS");
            builder.Entity<Category>().ToTable("CATEGORIES");
            builder.Entity<Product>().ToTable("PRODUCTS");
            builder.Entity<ProductVariant>().ToTable("PRODUCT_VARIANTS");
            builder.Entity<Cart>().ToTable("CARTS");
            builder.Entity<CartItem>().ToTable("CART_ITEMS");
            builder.Entity<Order>().ToTable("ORDERS");
            builder.Entity<OrderItem>().ToTable("ORDER_ITEMS");
            builder.Entity<Payment>().ToTable("PAYMENTS");
            builder.Entity<Review>().ToTable("REVIEWS");
            builder.Entity<AddressBook>().ToTable("ADDRESS_BOOK");

            // keys, relationships, indexes, unique constraints
            builder.Entity<Account>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Username).IsUnique();
            });

            builder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne<Account>()
                 .WithOne()
                 .HasForeignKey<User>(u => u.Id)
                 .HasConstraintName("FK_ACCOUNTS_USER");
            });

            // product-variant relationship
            builder.Entity<ProductVariant>()
                   .HasOne(pv => pv.Product)
                   .WithMany(p => p.Variants)
                   .HasForeignKey(pv => pv.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            // more mapping as needed...
        }
    }
}
