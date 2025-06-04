using System;
using System.Collections.Generic;
using ActivePulse.Entities;
using Microsoft.EntityFrameworkCore;

namespace ActivePulse.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CardData> CardData { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<DeliveryCategory> DeliveryCategories { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<PayOrder> PayOrders { get; set; }

    public virtual DbSet<PayStatus> PayStatuses { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductInSupply> ProductInSupplies { get; set; }

    public virtual DbSet<ProductSpecification> ProductSpecifications { get; set; }

    public virtual DbSet<ProductsInOrder> ProductsInOrders { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<Supply> Supplies { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Database=ActivePulse;Username=postgres;Password=1");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CardData>(entity =>
        {
            entity.ToTable("card_data");
            entity.HasKey(e => e.CardDataId);

            // Явно указываем соответствие свойства и столбца
            entity.Property(e => e.OrderId)
                  .HasColumnName("order_id");

            entity.HasKey(e => e.CardDataId)
          .HasName("card_data_pkey"); // Опционально - имя первичного ключа

            // Явное указание соответствий
            entity.Property(e => e.CardDataId)
                  .HasColumnName("card_data_id");
            
            entity.Property(e => e.CardNumber).HasColumnName("card_number");
            entity.Property(e => e.CardOwner).HasColumnName("card_owner");
            entity.Property(e => e.CardPeriod).HasColumnName("card_period");
            entity.Property(e => e.Cvv).HasColumnName("cvv");

            entity.HasOne(d => d.Order)
                  .WithMany()
                  .HasForeignKey(d => d.OrderId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("card_data_order_id_fkey");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryCode).HasName("country_pkey");

            entity.ToTable("country");

            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("country_code");
            entity.Property(e => e.CountryName)
                .HasMaxLength(56)
                .HasColumnName("country_name");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customer_pkey");

            entity.ToTable("customer");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.Lastname)
                .HasMaxLength(50)
                .HasColumnName("lastname");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(50)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");

            entity.HasOne(d => d.GenderNavigation).WithMany(p => p.Customers)
                .HasForeignKey(d => d.Gender)
                .HasConstraintName("customer_gender_fkey");
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.DeliveryId).HasName("delivery_pkey");

            entity.ToTable("delivery");

            entity.Property(e => e.DeliveryId).HasColumnName("delivery_id");
            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .HasColumnName("address");
            entity.Property(e => e.DeliveryCategoryId).HasColumnName("delivery_category_id");
            entity.Property(e => e.DeliveryDate).HasColumnName("delivery_date");
            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.HasOne(d => d.DeliveryCategory).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.DeliveryCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("delivery_delivery_category_id_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("delivery_order_id_fkey");
        });

        modelBuilder.Entity<DeliveryCategory>(entity =>
        {
            entity.HasKey(e => e.DeliveryCategoryId).HasName("delivery_category_pkey");

            entity.ToTable("delivery_category");

            entity.Property(e => e.DeliveryCategoryId).HasColumnName("delivery_category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("employee_pkey");

            entity.ToTable("employee");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(100)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");

            entity.HasOne(d => d.GenderNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Gender)
                .HasConstraintName("employee_gender_fkey");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.GenderId).HasName("gender_pkey");

            entity.ToTable("gender");

            entity.Property(e => e.GenderId)
                .HasMaxLength(1)
                .ValueGeneratedNever()
                .HasColumnName("gender_id");
            entity.Property(e => e.Gender1)
                .HasMaxLength(7)
                .HasColumnName("gender");
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.ManufacturerId).HasName("manufacturer_pkey");

            entity.ToTable("manufacturer");

            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("country_code");
            entity.Property(e => e.ManufacturerName)
                .HasMaxLength(20)
                .HasColumnName("manufacturer_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");

            entity.HasOne(d => d.CountryCodeNavigation).WithMany(p => p.Manufacturers)
                .HasForeignKey(d => d.CountryCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("manufacturer_country_code_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("order__pkey");

            entity.ToTable("order_");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.OrderStatus)
                .HasDefaultValue(1)
                .HasColumnName("order_status");
            entity.Property(e => e.PaymentMethod)
                .HasDefaultValue(1)
                .HasColumnName("payment_method");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.StoreId).HasColumnName("store_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order__customer_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.Orders)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order__employee_id_fkey");

            entity.HasOne(d => d.OrderStatusNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order__order_status_fkey");

            entity.HasOne(d => d.PaymentMethodNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order__payment_method_fkey");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order__store_id_fkey");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.OrderStatusId).HasName("order_status_pkey");

            entity.ToTable("order_status");

            entity.Property(e => e.OrderStatusId).HasColumnName("order_status_id");
            entity.Property(e => e.OrderStatusDescription)
                .HasMaxLength(15)
                .HasColumnName("order_status_description");
        });

        modelBuilder.Entity<PayOrder>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("pay_order");

            entity.Property(e => e.Card).HasColumnName("card");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PayStatus).HasColumnName("pay_status");

            entity.HasOne(d => d.Order).WithMany()
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_order_order_id_fkey");

            entity.HasOne(d => d.PayStatusNavigation).WithMany()
                .HasForeignKey(d => d.PayStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_order_pay_status_fkey");
        });

        modelBuilder.Entity<PayStatus>(entity =>
        {
            entity.HasKey(e => e.PayStatusId).HasName("pay_status_pkey");

            entity.ToTable("pay_status");

            entity.Property(e => e.PayStatusId).HasColumnName("pay_status_id");
            entity.Property(e => e.StatusDescription)
                .HasMaxLength(15)
                .HasColumnName("status_description");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("payment_method_pkey");

            entity.ToTable("payment_method");

            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("product_pkey");

            entity.ToTable("product");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.WarrantyPeriodInMonth).HasColumnName("warranty_period_in_month");

            entity.HasOne(d => d.CategoryNameNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_category_name_fkey");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Products)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_manufacturer_id_fkey");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("product_category_pkey");

            entity.ToTable("product_category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.SportId).HasColumnName("sport_id");

            entity.HasOne(d => d.Sport).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.SportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_category_sport_id_fkey");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("product_image_pkey");

            entity.ToTable("product_image");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_image_product_id_fkey");
        });

        modelBuilder.Entity<ProductInSupply>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("product_in_supply");

            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SupplyId).HasColumnName("supply_id");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_in_supply_product_id_fkey");
        });

        modelBuilder.Entity<ProductSpecification>(entity =>
        {
            entity.ToTable("product_specification");

            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.Gender)
                .HasMaxLength(6)
                .HasColumnName("gender");
            entity.Property(e => e.Material)
                .HasMaxLength(100)
                .HasColumnName("material");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Size)
                .HasMaxLength(10)
                .HasColumnName("size_");
            entity.Property(e => e.Weight)
                .HasPrecision(10, 2)
                .HasColumnName("weight");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_specification_product_id_fkey");
        });

        modelBuilder.Entity<ProductsInOrder>(entity =>
        {
            entity.ToTable("products_in_order");

            // Указываем составной первичный ключ
            entity.HasKey(e => new { e.ProductId, e.OrderId });

            entity.Property(e => e.Count).HasColumnName("count_");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Order)
                .WithMany()
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_in_order_order_id_fkey");

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_in_order_product_id_fkey");
        });

        modelBuilder.Entity<Sport>(entity =>
        {
            entity.HasKey(e => e.SportId).HasName("sport_pkey");

            entity.ToTable("sport");

            entity.Property(e => e.SportId).HasColumnName("sport_id");
            entity.Property(e => e.SportName)
                .HasMaxLength(40)
                .HasColumnName("sport_name");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("store_pkey");

            entity.ToTable("store");

            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ClosingTime).HasColumnName("closing_time");
            entity.Property(e => e.OpeningTime).HasColumnName("opening_time");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.StoreAddress)
                .HasMaxLength(150)
                .HasColumnName("store_address");
            entity.Property(e => e.StoreName)
                .HasMaxLength(120)
                .HasColumnName("store_name");
        });

        modelBuilder.Entity<Supply>(entity =>
        {
            entity.HasKey(e => e.SupplyId).HasName("supply_pkey");

            entity.ToTable("supply");

            entity.Property(e => e.SupplyId).HasColumnName("supply_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.SupplyDate).HasColumnName("supply_date");

            entity.HasOne(d => d.Store).WithMany(p => p.Supplies)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("supply_store_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user__pkey");

            entity.ToTable("user_");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Login)
                .HasMaxLength(30)
                .HasColumnName("login");
            entity.Property(e => e.Password).HasColumnName("password_");
            entity.Property(e => e.RoleId)
                .HasMaxLength(1)
                .HasColumnName("role_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Users)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("user__customer_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("user__employee_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("user__role_id_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("user_role_pkey");

            entity.ToTable("user_role");

            entity.Property(e => e.RoleId)
                .HasMaxLength(1)
                .ValueGeneratedNever()
                .HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(15)
                .HasColumnName("role_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
