///*
//Generate full C# EF Core 9 entity classes for an e-commerce app with Oracle DB.
//Requirements:
//- Map 15 tables: users, accounts, address_book, shops, categories, products, product_variants, media, carts, cart_items, orders, payments, order_items, reviews, delivery_reviews
//- Column names are snake_case in Oracle, convert to PascalCase properties in C#
//- Include proper data types (int, string, decimal, DateTime, bool)
//- Include navigation properties:
//    - 1-n, n-1 relationships based on foreign keys
//    - Collections for multiple related entities
//- Use nullable types for optional columns
//- Include comments for clarity
//- Ready to paste in ASP.NET Core project using EF Core 9
//*/

//using System;
//using System.Collections.Generic;

//namespace ECommerceApp.Models
//{
//    // ===============================
//    // Users
//    // ===============================
//    /// <summary>
//    /// Users table: stores application users.
//    /// </summary>
//    [Table("users")]
//    public class User
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("email")]
//        public string Email { get; set; } = null!;

//        [Column("full_name")]
//        public string? FullName { get; set; }

//        [Column("phone")]
//        public string? Phone { get; set; }

//        [Required]
//        [Column("role")]
//        public string Role { get; set; } = "customer";

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        // Navigation properties
//        public ICollection<Account> Accounts { get; set; } = new List<Account>();
//        public ICollection<AddressBook> AddressBooks { get; set; } = new List<AddressBook>();
//        public ICollection<Shop> Shops { get; set; } = new List<Shop>();
//        public ICollection<Order> Orders { get; set; } = new List<Order>();
//        public ICollection<Review> Reviews { get; set; } = new List<Review>();
//        public ICollection<DeliveryReview> DeliveryReviews { get; set; } = new List<DeliveryReview>();
//        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
//    }

//    // ===============================
//    // Accounts
//    // ===============================
//    /// <summary>
//    /// Accounts linked to users (credentials).
//    /// </summary>
//    [Table("accounts")]
//    public class Account
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("username")]
//        public string Username { get; set; } = null!;

//        [Required]
//        [Column("password_hash")]
//        public string PasswordHash { get; set; } = null!;

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [Required]
//        [Column("user_id")]
//        public int UserId { get; set; }

//        [ForeignKey(nameof(UserId))]
//        public User User { get; set; } = null!;
//    }

//    // ===============================
//    // AddressBook
//    // ===============================
//    /// <summary>
//    /// User addresses for shipping/billing.
//    /// </summary>
//    [Table("address_book")]
//    public class AddressBook
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("user_id")]
//        public int UserId { get; set; }

//        [Column("full_name")]
//        public string? FullName { get; set; }

//        [Column("phone")]
//        public string? Phone { get; set; }

//        [Column("address_line")]
//        public string? AddressLine { get; set; }

//        [Column("ward")]
//        public string? Ward { get; set; }

//        [Column("district")]
//        public string? District { get; set; }

//        [Column("city")]
//        public string? City { get; set; }

//        [Required]
//        [Column("is_default")]
//        public bool IsDefault { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(UserId))]
//        public User User { get; set; } = null!;
//    }

//    // ===============================
//    // Shops
//    // ===============================
//    /// <summary>
//    /// Merchant shops owned by users.
//    /// </summary>
//    [Table("shops")]
//    public class Shop
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("owner_id")]
//        public int OwnerId { get; set; }

//        [Required]
//        [Column("name")]
//        public string Name { get; set; } = null!;

//        [Column("description")]
//        public string? Description { get; set; }

//        [Column("logo")]
//        public string? Logo { get; set; }

//        [Required]
//        [Column("status")]
//        public string Status { get; set; } = "active";

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(OwnerId))]
//        public User Owner { get; set; } = null!;

//        public ICollection<Product> Products { get; set; } = new List<Product>();
//    }

//    // ===============================
//    // Categories
//    // ===============================
//    /// <summary>
//    /// Product categories (self-referencing for hierarchy).
//    /// </summary>
//    [Table("categories")]
//    public class Category
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("name")]
//        public string Name { get; set; } = null!;

//        [Column("description")]
//        public string? Description { get; set; }

//        [Column("parent_id")]
//        public int? ParentId { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(ParentId))]
//        public Category? Parent { get; set; }

//        public ICollection<Category> Children { get; set; } = new List<Category>();

//        public ICollection<Product> Products { get; set; } = new List<Product>();
//    }

//    // ===============================
//    // Products
//    // ===============================
//    /// <summary>
//    /// Products listed within shops.
//    /// </summary>
//    [Table("products")]
//    public class Product
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("shop_id")]
//        public int ShopId { get; set; }

//        [Required]
//        [Column("category_id")]
//        public int CategoryId { get; set; }

//        [Required]
//        [Column("name")]
//        public string Name { get; set; } = null!;

//        [Column("description")]
//        public string? Description { get; set; }

//        [Column("brand")]
//        public string? Brand { get; set; }

//        [Required]
//        [Column("status")]
//        public string Status { get; set; } = "active";

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(ShopId))]
//        public Shop Shop { get; set; } = null!;

//        [ForeignKey(nameof(CategoryId))]
//        public Category Category { get; set; } = null!;

//        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
//        public ICollection<Media> Media { get; set; } = new List<Media>();
//    }

//    // ===============================
//    // ProductVariants
//    // ===============================
//    /// <summary>
//    /// Specific variants of a product (size, color, SKU, price, quantity).
//    /// </summary>
//    [Table("product_variants")]
//    public class ProductVariant
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("product_id")]
//        public int ProductId { get; set; }

//        [Required]
//        [Column("sku")]
//        public string SKU { get; set; } = null!;

//        [Column("variant_size")]
//        public string? VariantSize { get; set; }

//        [Column("color")]
//        public string? Color { get; set; }

//        [Column("material")]
//        public string? Material { get; set; }

//        [Required]
//        [Column("price")]
//        public decimal Price { get; set; }

//        [Required]
//        [Column("quantity")]
//        public int Quantity { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(ProductId))]
//        public Product Product { get; set; } = null!;

//        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
//        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
//        public ICollection<Review> Reviews { get; set; } = new List<Review>();
//        public ICollection<Media> Media { get; set; } = new List<Media>();
//    }

//    // ===============================
//    // Media
//    // ===============================
//    /// <summary>
//    /// Media records (images) related to different entities; EntityType used to indicate owner.
//    /// </summary>
//    [Table("media")]
//    public class Media
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        // Example values for EntityType: "product", "variant", "shop", "user"
//        [Column("entity_type")]
//        public string? EntityType { get; set; }

//        // Stores the id of the related entity; nullable because some media may be transient
//        [Column("entity_id")]
//        public int? EntityId { get; set; }

//        [Column("image_url")]
//        public string? ImageUrl { get; set; }

//        [Column("public_id")]
//        public string? PublicId { get; set; }

//        [Required]
//        [Column("is_primary")]
//        public bool IsPrimary { get; set; }

//        [Column("alt_text")]
//        public string? AltText { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        // Note: polymorphic navigation not modeled here; use EntityType + EntityId to resolve.
//    }

//    // ===============================
//    // Carts
//    // ===============================
//    /// <summary>
//    /// Shopping carts belonging to users.
//    /// </summary>
//    [Table("carts")]
//    public class Cart
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("user_id")]
//        public int UserId { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(UserId))]
//        public User User { get; set; } = null!;

//        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
//    }

//    // ===============================
//    // CartItems
//    // ===============================
//    /// <summary>
//    /// Items inside a cart referencing a product variant.
//    /// </summary>
//    [Table("cart_items")]
//    public class CartItem
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("cart_id")]
//        public int CartId { get; set; }

//        [Required]
//        [Column("variant_id")]
//        public int VariantId { get; set; }

//        [Required]
//        [Column("quantity")]
//        public int Quantity { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(CartId))]
//        public Cart Cart { get; set; } = null!;

//        [ForeignKey(nameof(VariantId))]
//        public ProductVariant Variant { get; set; } = null!;
//    }

//    // ===============================
//    // Orders
//    // ===============================
//    /// <summary>
//    /// Orders placed by users.
//    /// </summary>
//    [Table("orders")]
//    public class Order
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("user_id")]
//        public int UserId { get; set; }

//        [Required]
//        [Column("status")]
//        public string Status { get; set; } = "pending";

//        [Column("total")]
//        public decimal? Total { get; set; }

//        [Column("shipping_address_id")]
//        public int? ShippingAddressId { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(UserId))]
//        public User User { get; set; } = null!;

//        [ForeignKey(nameof(ShippingAddressId))]
//        public AddressBook? ShippingAddress { get; set; }

//        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
//        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
//        public ICollection<DeliveryReview> DeliveryReviews { get; set; } = new List<DeliveryReview>();
//    }

//    // ===============================
//    // Payments
//    // ===============================
//    /// <summary>
//    /// Payments related to orders.
//    /// </summary>
//    [Table("payments")]
//    public class Payment
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("order_id")]
//        public int OrderId { get; set; }

//        [Required]
//        [Column("method")]
//        public string Method { get; set; } = null!;

//        [Required]
//        [Column("status")]
//        public string Status { get; set; } = "pending";

//        [Column("transaction_id")]
//        public string? TransactionId { get; set; }

//        [Required]
//        [Column("amount")]
//        public decimal Amount { get; set; }

//        [Column("paid_at")]
//        public DateTime? PaidAt { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(OrderId))]
//        public Order Order { get; set; } = null!;
//    }

//    // ===============================
//    // OrderItems
//    // ===============================
//    /// <summary>
//    /// Items in an order referencing product variants.
//    /// </summary>
//    [Table("order_items")]
//    public class OrderItem
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("order_id")]
//        public int OrderId { get; set; }

//        [Required]
//        [Column("variant_id")]
//        public int VariantId { get; set; }

//        [Required]
//        [Column("quantity")]
//        public int Quantity { get; set; }

//        [Required]
//        [Column("unit_price")]
//        public decimal UnitPrice { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(OrderId))]
//        public Order Order { get; set; } = null!;

//        [ForeignKey(nameof(VariantId))]
//        public ProductVariant Variant { get; set; } = null!;
//    }

//    // ===============================
//    // Reviews
//    // ===============================
//    /// <summary>
//    /// Product variant reviews by users.
//    /// </summary>
//    [Table("reviews")]
//    public class Review
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("user_id")]
//        public int UserId { get; set; }

//        [Required]
//        [Column("variant_id")]
//        public int VariantId { get; set; }

//        [Required]
//        [Column("rating")]
//        public decimal Rating { get; set; }

//        [Column("comment_text")]
//        public string? CommentText { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(UserId))]
//        public User User { get; set; } = null!;

//        [ForeignKey(nameof(VariantId))]
//        public ProductVariant Variant { get; set; } = null!;
//    }

//    // ===============================
//    // DeliveryReviews
//    // ===============================
//    /// <summary>
//    /// Reviews for delivery experience per order.
//    /// </summary>
//    [Table("delivery_reviews")]
//    public class DeliveryReview
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("user_id")]
//        public int UserId { get; set; }

//        [Required]
//        [Column("order_id")]
//        public int OrderId { get; set; }

//        [Required]
//        [Column("rating")]
//        public decimal Rating { get; set; }

//        [Column("comment_text")]
//        public string? CommentText { get; set; }

//        [Required]
//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        [ForeignKey(nameof(UserId))]
//        public User User { get; set; } = null!;

//        [ForeignKey(nameof(OrderId))]
//        public Order Order { get; set; } = null!;
//    }
//}
