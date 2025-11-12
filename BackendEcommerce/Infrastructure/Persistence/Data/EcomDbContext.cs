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
        public DbSet<AdministrativeRegion> AdministrativeRegions { get; set; }
        public DbSet<AdministrativeUnit> AdministrativeUnits { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }


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
                entity.Property(e => e.Status).HasColumnName("STATUS");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
                entity.Property(s => s.CccdStatus).HasColumnName("CCCD_STATUS").IsRequired();
                entity.Property(s => s.BankStatus).HasColumnName("BANK_STATUS").IsRequired();
                entity.Property(s => s.BankAccountNumber).HasColumnName("BANK_ACCOUNT_NUMBER");

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
                entity.Property(e => e.VariantCount).HasColumnName("VARIANT_COUNT");
                entity.Property(e => e.MinPrice).HasColumnName("MIN_PRICE").HasPrecision(12, 2);
                entity.Property(p => p.SelledCount).HasColumnName("SELLED_COUNT").HasDefaultValue(0);

                entity.Property(p => p.ReviewCount).HasColumnName("REVIEW_COUNT").HasDefaultValue(0);

                entity.Property(p => p.AverageRating).HasColumnName("AVERAGE_RATING").HasDefaultValue(0.0).HasPrecision(3, 2);
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
                entity.Property(e => e.EntityType).HasColumnName("ENTITY_TYPE").IsUnicode(false);
                entity.Property(e => e.EntityId).HasColumnName("ENTITY_ID");
                entity.Property(e => e.ImageUrl).HasColumnName("IMAGE_URL").IsUnicode(false);
                entity.Property(e => e.PublicId).HasColumnName("PUBLIC_ID").IsUnicode(false);
                entity.Property(e => e.IsPrimary).HasColumnName("IS_PRIMARY");
                entity.Property(e => e.AltText).HasColumnName("ALT_TEXT").IsUnicode(false);
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
            modelBuilder.Entity<Order>(builder =>
            {
                builder.ToTable("ORDERS");
                builder.HasKey(o => o.Id);

                // Cấu hình FK đến User
                builder.HasOne(o => o.User)
                    .WithMany(o => o.Orders) // (Giả định User không cần list Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // (Không cho xóa User nếu còn Order)
                builder.Property(o => o.UserId).HasColumnName("USERID");

                builder.Property(o => o.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(o => o.PaymentMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(o => o.TotalAmount)
                    .HasColumnType("NUMBER(18,2)");

                // === CẤU HÌNH CÁC CỘT SNAPSHOT ĐỊA CHỈ ===

                builder.Property(o => o.Shipping_FullName)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(o => o.Shipping_Phone)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(o => o.Shipping_AddressLine)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(o => o.Shipping_Ward)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(o => o.Shipping_District)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(o => o.Shipping_City)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsRequired();

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
            modelBuilder.Entity<OrderItem>(builder =>
            {
                builder.ToTable("ORDER_ITEMS");
                builder.HasKey(oi => oi.Id);

                // Cấu hình Khóa ngoại (FK) đến Order
                builder.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade); // (Nếu Xóa Order -> Xóa Item)
                // 1. Cấu hình Khóa ngoại (FK) đến ProductVariant
                builder.HasOne(oi => oi.Variant)
                    .WithMany() // (Giả định Variant không cần list OrderItem)
                    .HasForeignKey(oi => oi.ProductVariantId) // (Dùng cột Nullable)
                    .IsRequired(false) // (Cho phép Null)
                    .OnDelete(DeleteBehavior.SetNull); // <-- MẤU CHỐT QUAN TRỌNG NHẤT
                                                       // (Khi Xóa Cứng Variant -> Cột này = NULL)
                builder.Property(oi => oi.ProductVariantId).HasColumnName("PRODUCT_VARIANT_ID");

                // 2. Cấu hình các cột Snapshot mới (VARCHAR2 / Non-Unicode)
                builder.Property(oi => oi.Sku)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(oi => oi.ProductName)
                    .HasMaxLength(255)
                    .IsUnicode(false) // (Giả định Tên SP là VARCHAR2)
                    .IsRequired();

                builder.Property(oi => oi.VariantName)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(oi => oi.ImageUrl)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(oi => oi.PriceAtTimeOfPurchase)
                    .HasColumnType("NUMBER(18,2)"); // (Hoặc NUMBER(18,2) tùy Oracle)

                // === KẾT THÚC CẤU HÌNH ===
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
                entity.Property(e => e.OrderItemId).HasColumnName("ORDER_ITEM_ID");
                entity.HasOne(r => r.OrderItem)
                 .WithMany() // Assuming OrderItem does not need a collection of Reviews
                 .HasForeignKey(r => r.OrderItemId)
                 .IsRequired();
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
                entity.Property(e => e.RevokedByIp).HasColumnName("REVOKED_BY_IP");
                entity.Ignore(e => e.Revoked);
                entity.Ignore(e => e.IsExpired);
                entity.Ignore(e => e.IsActive);

                entity.HasOne(e => e.Account)
                    .WithMany(r => r.RefreshTokens)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AdministrativeRegion>(builder =>
            {
                builder.ToTable("ADMINISTRATIVE_REGIONS");
                builder.HasKey(ar => ar.Id);
                builder.Property(ar => ar.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                builder.Property(ar => ar.Name)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME");

                builder.Property(ar => ar.NameEn)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME_EN");

                builder.Property(ar => ar.CodeName)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("CODE_NAME");

                builder.Property(ar => ar.CodeNameEn)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("CODE_NAME_EN");
            });

            modelBuilder.Entity<AdministrativeUnit>(builder =>
            {
                builder.ToTable("ADMINISTRATIVE_UNITS");
                builder.HasKey(au => au.Id);
                builder.Property(au => au.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                builder.Property(au => au.FullName)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("FULL_NAME");

                builder.Property(au => au.FullNameEn)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("FULL_NAME_EN");

                builder.Property(au => au.CodeName)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("CODE_NAME");

                builder.Property(au => au.CodeNameEn)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("CODE_NAME_EN");

                builder.Property(au => au.ShortName)
                    .HasMaxLength(100).IsUnicode(false)
                    .HasColumnName("SHORT_NAME");

                builder.Property(au => au.ShortNameEn)
                    .HasMaxLength(100).IsUnicode(false)
                    .HasColumnName("SHORT_NAME_EN");
            });

            modelBuilder.Entity<Province>(builder =>
            {
                builder.ToTable("PROVINCES");
                builder.HasKey(p => p.Code);
                builder.Property(p => p.Code)
                    .HasMaxLength(20).IsUnicode(false)
                    .HasColumnName("CODE");

                builder.Property(p => p.Name)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME");

                builder.Property(p => p.NameEn)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME_EN");

                builder.Property(p => p.FullName)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("FULL_NAME");

                builder.Property(p => p.FullNameEn)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("FULL_NAME_EN");

                builder.Property(p => p.CodeName)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("CODE_NAME");

                builder.Property(p => p.AdministrativeUnitId)
                    .HasColumnName("ADMINISTRATIVE_UNIT_ID");

                builder.Property(p => p.AdministrativeRegionId)
                    .HasColumnName("ADMINISTRATIVE_REGION_ID");

                builder.HasOne(p => p.AdministrativeUnit)
                    .WithMany()
                    .HasForeignKey(p => p.AdministrativeUnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(p => p.AdministrativeRegion)
                    .WithMany(r => r.Provinces)
                    .HasForeignKey(p => p.AdministrativeRegionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<District>(builder =>
            {
                builder.ToTable("DISTRICTS");
                builder.HasKey(d => d.Code);
                builder.Property(d => d.Code)
                    .HasMaxLength(20).IsUnicode(false)
                    .HasColumnName("CODE");

                builder.Property(d => d.Name)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME");

                builder.Property(d => d.NameEn)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME_EN");

                builder.Property(d => d.FullName)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("FULL_NAME");

                builder.Property(d => d.FullNameEn)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("FULL_NAME_EN");

                builder.Property(d => d.CodeName)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("CODE_NAME");

                builder.Property(d => d.AdministrativeUnitId)
                    .HasColumnName("ADMINISTRATIVE_UNIT_ID");

                builder.Property(d => d.ProvinceCode)
                    .HasColumnName("PROVINCE_CODE");

                builder.HasOne(d => d.AdministrativeUnit)
                    .WithMany()
                    .HasForeignKey(d => d.AdministrativeUnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(d => d.Province)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.ProvinceCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ward>(builder =>
            {
                builder.ToTable("WARDS");
                builder.HasKey(w => w.Code);
                builder.Property(w => w.Code)
                    .HasMaxLength(20).IsUnicode(false)
                    .HasColumnName("CODE");

                builder.Property(w => w.Name)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME");

                builder.Property(w => w.NameEn)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("NAME_EN");

                builder.Property(w => w.FullName)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("FULL_NAME");

                builder.Property(w => w.FullNameEn)
                    .HasMaxLength(255).IsUnicode(false).IsRequired()
                    .HasColumnName("FULL_NAME_EN");

                builder.Property(w => w.CodeName)
                    .HasMaxLength(255).IsUnicode(false)
                    .HasColumnName("CODE_NAME");

                builder.Property(w => w.AdministrativeUnitId)
                    .HasColumnName("ADMINISTRATIVE_UNIT_ID");

                builder.Property(w => w.DistrictCode)
                    .HasColumnName("DISTRICT_CODE");

                builder.HasOne(w => w.AdministrativeUnit)
                    .WithMany()
                    .HasForeignKey(w => w.AdministrativeUnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(w => w.District)
                    .WithMany(d => d.Wards)
                    .HasForeignKey(w => w.DistrictCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
