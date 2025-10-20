using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using ECommerceApp.Models;

namespace ECommerceApp.Data
{
    public class EcomDbContext : DbContext
    {
        public EcomDbContext(DbContextOptions<EcomDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AddressBook> AddressBooks { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<DeliveryReview> DeliveryReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // ===============================
            // Users
            // ===============================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Role).HasColumnName("role").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasMany(e => e.Accounts).WithOne(a => a.User).HasForeignKey(a => a.UserId);
                entity.HasMany(e => e.AddressBooks).WithOne(ab => ab.User).HasForeignKey(ab => ab.UserId);
                entity.HasMany(e => e.Shops).WithOne(s => s.Owner).HasForeignKey(s => s.OwnerId);
                entity.HasMany(e => e.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId);
                entity.HasMany(e => e.Reviews).WithOne(r => r.User).HasForeignKey(r => r.UserId);
                entity.HasMany(e => e.DeliveryReviews).WithOne(dr => dr.User).HasForeignKey(dr => dr.UserId);
                entity.HasMany(e => e.Carts).WithOne(c => c.User).HasForeignKey(c => c.UserId);
            });

            // ===============================
            // Accounts
            // ===============================
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username").IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("passwordhash").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            // ===============================
            // AddressBook
            // ===============================
            modelBuilder.Entity<AddressBook>(entity =>
            {
                entity.ToTable("address_book");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.AddressLine).HasColumnName("address_line");
                entity.Property(e => e.Ward).HasColumnName("ward");
                entity.Property(e => e.District).HasColumnName("district");
                entity.Property(e => e.City).HasColumnName("city");
                entity.Property(e => e.IsDefault).HasColumnName("is_default");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // ===============================
            // Shops
            // ===============================
            modelBuilder.Entity<Shop>(entity =>
            {
                entity.ToTable("shops");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OwnerId).HasColumnName("owner_id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Logo).HasColumnName("logo");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasMany(s => s.Products).WithOne(p => p.Shop).HasForeignKey(p => p.ShopId);
            });

            // ===============================
            // Categories
            // ===============================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ParentId).HasColumnName("parent_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId);
            });

            // ===============================
            // Products
            // ===============================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ShopId).HasColumnName("shop_id");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Brand).HasColumnName("brand");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // ===============================
            // ProductVariants
            // ===============================
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("product_variants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.SKU).HasColumnName("sku").IsRequired();
                entity.Property(e => e.VariantSize).HasColumnName("variant_size");
                entity.Property(e => e.Color).HasColumnName("color");
                entity.Property(e => e.Material).HasColumnName("material");
                entity.Property(e => e.Price).HasColumnName("price").HasPrecision(12,2);
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // ===============================
            // Media
            // ===============================
            modelBuilder.Entity<Media>(entity =>
            {
                entity.ToTable("media");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.EntityType).HasColumnName("entity_type");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.PublicId).HasColumnName("public_id");
                entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
                entity.Property(e => e.AltText).HasColumnName("alt_text");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // ===============================
            // Carts
            // ===============================
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("carts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasMany(c => c.CartItems).WithOne(ci => ci.Cart).HasForeignKey(ci => ci.CartId);
            });

            // ===============================
            // CartItems
            // =================
            // ===============================
            // CartItems
            // ===============================
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("cart_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CartId).HasColumnName("cart_id");
                entity.Property(e => e.VariantId).HasColumnName("variant_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // ===============================
            // Orders
            // ===============================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.Total).HasColumnName("total").HasPrecision(12, 2);
                entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasMany(o => o.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId);
                entity.HasMany(o => o.Payments).WithOne(p => p.Order).HasForeignKey(p => p.OrderId);
                entity.HasMany(o => o.DeliveryReviews).WithOne(dr => dr.Order).HasForeignKey(dr => dr.OrderId);
            });

            // ===============================
            // Payments
            // ===============================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.Method).HasColumnName("method").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
                entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2);
                entity.Property(e => e.PaidAt).HasColumnName("paid_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // ===============================
            // OrderItems
            // ===============================
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.VariantId).HasColumnName("variant_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 2);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // ===============================
            // Reviews
            // ===============================
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.VariantId).HasColumnName("variant_id");
                entity.Property(e => e.Rating).HasColumnName("rating").HasPrecision(2,1);
                entity.Property(e => e.CommentText).HasColumnName("comment_text");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // ===============================
            // DeliveryReviews
            // ===============================
            modelBuilder.Entity<DeliveryReview>(entity =>
            {
                entity.ToTable("delivery_reviews");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.Rating).HasColumnName("rating").HasPrecision(2,1);
                entity.Property(e => e.CommentText).HasColumnName("comment_text");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });
        }
    }
}

