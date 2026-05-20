using Microsoft.EntityFrameworkCore;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence;

public sealed class LendingDbContext(DbContextOptions<LendingDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<BorrowerDetailEntity> BorrowerDetails => Set<BorrowerDetailEntity>();
    public DbSet<MediaEntity> Media => Set<MediaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users", table =>
            {
                table.HasCheckConstraint("ck_users_name_english_letters", "name ~ '^[A-Za-z0-9]+$'");
            });
            entity.HasKey(x => x.UserId).HasName("users_pkey");
            entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("users_email_key");
            entity.HasIndex(x => x.Name).IsUnique().HasDatabaseName("users_name_key");
            entity.HasIndex(x => new { x.AuthProvider, x.ProviderUserId })
                .IsUnique()
                .HasDatabaseName("users_auth_provider_provider_user_id_key");
            entity.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(x => x.AuthProvider)
                .HasColumnName("auth_provider")
                .HasMaxLength(50)
                .HasDefaultValue("local")
                .IsRequired();
            entity.Property(x => x.ProviderUserId)
                .HasColumnName("provider_user_id")
                .HasMaxLength(255);
            entity.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            entity.Property(x => x.Role)
                .HasColumnName("role")
                .HasDefaultValue("user")
                .ValueGeneratedOnAdd();
            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ItemEntity>(entity =>
        {
            entity.ToTable("items");
            entity.HasKey(x => x.ItemId).HasName("items_pkey");
            entity.Property(x => x.ItemId).HasColumnName("item_id").ValueGeneratedOnAdd();
            entity.Property(x => x.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(x => x.ObjectName).HasColumnName("object_name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Maker).HasColumnName("maker").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Material).HasColumnName("material").HasMaxLength(100).IsRequired();
            entity.Property(x => x.CurrentStatus)
                .HasColumnName("current_status")
                .HasMaxLength(50)
                .HasDefaultValue("Available")
                .ValueGeneratedOnAdd();
            entity.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(300).HasDefaultValue(null);
            entity.Property(x => x.Description).HasColumnName("description").IsRequired();
            entity.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.OrderId).HasName("orders_pkey");
            entity.Property(x => x.OrderId).HasColumnName("order_id").ValueGeneratedOnAdd();
            entity.Property(x => x.BorrowerDetailId).HasColumnName("borrower_detail_id").IsRequired();
            entity.Property(x => x.ObjectId).HasColumnName("item_id").IsRequired();
            entity.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
            entity.Property(x => x.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
            entity.Property(x => x.ActualReturnDate).HasColumnName("actual_return_date").HasColumnType("date");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.BorrowerDetail)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.BorrowerDetailId)
                .HasConstraintName("orders_borrower_detail_id_fkey")
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Item)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.ObjectId)
                .HasConstraintName("orders_item_id_fkey")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BorrowerDetailEntity>(entity =>
        {
            entity.ToTable("borrower_details");
            entity.HasKey(x => x.BorrowerDetailId).HasName("borrower_details_pkey");
            entity.Property(x => x.BorrowerDetailId).HasColumnName("borrower_detail_id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.BorrowerName).HasColumnName("borrower_name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Link).HasColumnName("link").HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("date").IsRequired();
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("date").IsRequired();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .HasConstraintName("borrower_details_user_id_fkey")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MediaEntity>(entity =>
        {
            entity.ToTable("media");
            entity.HasKey(x => x.MediaId).HasName("media_pkey");
            entity.HasIndex(x => x.OrderId).HasDatabaseName("idx_media_order_id");
            entity.Property(x => x.MediaId).HasColumnName("media_id").ValueGeneratedOnAdd();
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.ObjectId).HasColumnName("object_id").IsRequired();
            entity.Property(x => x.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Url).HasColumnName("url").IsRequired();
            entity.Property(x => x.Link).HasColumnName("link");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Media)
                .HasForeignKey(x => x.OrderId)
                .HasConstraintName("fk_media_order")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Item)
                .WithMany(x => x.Media)
                .HasForeignKey(x => x.ObjectId)
                .HasConstraintName("fk_media_item")
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
