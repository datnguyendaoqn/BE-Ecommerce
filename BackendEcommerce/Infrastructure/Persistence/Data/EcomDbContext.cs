using Microsoft.EntityFrameworkCore;
using BackendEcommerce.Infrastructure.Persistence.Models;

namespace BackendEcommerce.Infrastructure.Persistence.Data
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
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================
            // Users
            // ===============================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Email).HasColumnName("EMAIL").IsRequired();
                entity.Property(e => e.FullName).HasColumnName("FULL_NAME");
                entity.Property(e => e.Phone).HasColumnName("PHONE");
                entity.Property(e => e.Role).HasColumnName("ROLE").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

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
                entity.ToTable("ACCOUNTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Username).HasColumnName("USERNAME").IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("PASSWORDHASH").IsRequired();
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // AddressBook
            // ===============================
            modelBuilder.Entity<AddressBook>(entity =>
            {
                entity.ToTable("ADDRESS_BOOK");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.FullName).HasColumnName("FULL_NAME");
                entity.Property(e => e.Phone).HasColumnName("PHONE");
                entity.Property(e => e.AddressLine).HasColumnName("ADDRESS_LINE");
                entity.Property(e => e.Ward).HasColumnName("WARD");
                entity.Property(e => e.District).HasColumnName("DISTRICT");
                entity.Property(e => e.City).HasColumnName("CITY");
                entity.Property(e => e.IsDefault).HasColumnName("IS_DEFAULT");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // Shops
            // ===============================
            modelBuilder.Entity<Shop>(entity =>
            {
                entity.ToTable("SHOPS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.OwnerId).HasColumnName("OWNER_ID");
                entity.Property(e => e.Name).HasColumnName("NAME").IsRequired();
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
                entity.Property(e => e.Logo).HasColumnName("LOGO");
                entity.Property(e => e.Status).HasColumnName("STATUS");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

                entity.HasMany(s => s.Products).WithOne(p => p.Shop).HasForeignKey(p => p.ShopId);
            });

            // ===============================
            // Categories
            // ===============================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("CATEGORIES");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").IsRequired();
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
                entity.Property(e => e.ParentId).HasColumnName("PARENT_ID");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

                entity.HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId);
            });

            // ===============================
            // Products
            // ===============================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("PRODUCTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ShopId).HasColumnName("SHOP_ID");
                entity.Property(e => e.CategoryId).HasColumnName("CATEGORY_ID");
                entity.Property(e => e.Name).HasColumnName("NAME").IsRequired();
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
                entity.Property(e => e.Brand).HasColumnName("BRAND");
                entity.Property(e => e.Status).HasColumnName("STATUS");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // ProductVariants
            // ===============================
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("PRODUCT_VARIANTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ProductId).HasColumnName("PRODUCT_ID");
                entity.Property(e => e.SKU).HasColumnName("SKU").IsRequired();
                entity.Property(e => e.VariantSize).HasColumnName("VARIANT_SIZE");
                entity.Property(e => e.Color).HasColumnName("COLOR");
                entity.Property(e => e.Material).HasColumnName("MATERIAL");
                entity.Property(e => e.Price).HasColumnName("PRICE").HasPrecision(12, 2);
                entity.Property(e => e.Quantity).HasColumnName("QUANTITY");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // Media
            // ===============================
            modelBuilder.Entity<Media>(entity =>
            {
                entity.ToTable("MEDIA");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.EntityType).HasColumnName("ENTITY_TYPE");
                entity.Property(e => e.EntityId).HasColumnName("ENTITY_ID");
                entity.Property(e => e.ImageUrl).HasColumnName("IMAGE_URL");
                entity.Property(e => e.PublicId).HasColumnName("PUBLIC_ID");
                entity.Property(e => e.IsPrimary).HasColumnName("IS_PRIMARY");
                entity.Property(e => e.AltText).HasColumnName("ALT_TEXT");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            });

            // ===============================
            // Carts
            // ===============================
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("CARTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

                entity.HasMany(c => c.CartItems).WithOne(ci => ci.Cart).HasForeignKey(ci => ci.CartId);
            });

            // ===============================
            // CartItems
            // ===============================
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CART_ITEMS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CartId).HasColumnName("CART_ID");
                entity.Property(e => e.VariantId).HasColumnName("VARIANT_ID");
                entity.Property(e => e.Quantity).HasColumnName("QUANTITY");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // Orders
            // ===============================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("ORDERS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.Status).HasColumnName("STATUS");
                entity.Property(e => e.Total).HasColumnName("TOTAL").HasPrecision(12, 2);
                entity.Property(e => e.ShippingAddressId).HasColumnName("SHIPPING_ADDRESS_ID");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

                entity.HasMany(o => o.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId);
                entity.HasMany(o => o.Payments).WithOne(p => p.Order).HasForeignKey(p => p.OrderId);
                entity.HasMany(o => o.DeliveryReviews).WithOne(dr => dr.Order).HasForeignKey(dr => dr.OrderId);
            });

            // ===============================
            // Payments
            // ===============================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("PAYMENTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.OrderId).HasColumnName("ORDER_ID");
                entity.Property(e => e.Method).HasColumnName("METHOD").IsRequired();
                entity.Property(e => e.Status).HasColumnName("STATUS");
                entity.Property(e => e.TransactionId).HasColumnName("TRANSACTION_ID");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT").HasPrecision(12, 2);
                entity.Property(e => e.PaidAt).HasColumnName("PAID_AT");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // OrderItems
            // ===============================
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("ORDER_ITEMS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.OrderId).HasColumnName("ORDER_ID");
                entity.Property(e => e.VariantId).HasColumnName("VARIANT_ID");
                entity.Property(e => e.Quantity).HasColumnName("QUANTITY");
                entity.Property(e => e.UnitPrice).HasColumnName("UNIT_PRICE").HasPrecision(12, 2);
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // Reviews
            // ===============================
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("REVIEWS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.VariantId).HasColumnName("VARIANT_ID");
                entity.Property(e => e.Rating).HasColumnName("RATING").HasPrecision(2, 1);
                entity.Property(e => e.CommentText).HasColumnName("COMMENT_TEXT");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // DeliveryReviews
            // ===============================
            modelBuilder.Entity<DeliveryReview>(entity =>
            {
                entity.ToTable("DELIVERY_REVIEWS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.OrderId).HasColumnName("ORDER_ID");
                entity.Property(e => e.Rating).HasColumnName("RATING").HasPrecision(2, 1);
                entity.Property(e => e.CommentText).HasColumnName("COMMENT_TEXT");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
            });

            // ===============================
            // RefreshTokens
            // ===============================
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("REFRESH_TOKENS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AccountId).HasColumnName("ACCOUNT_ID");
                entity.Property(e => e.Token).HasColumnName("TOKEN");
                entity.Property(e => e.Expires).HasColumnName("EXPIRES");
                entity.Property(e => e.RevokedValue).HasColumnName("REVOKED");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.CreatedByIp).HasColumnName("CREATED_BY_IP");
                entity.Property(e => e.RevokedAt).HasColumnName("REVOKED_AT");

                entity.HasOne(e => e.Account)
                    .WithMany(r => r.RefreshTokens)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
