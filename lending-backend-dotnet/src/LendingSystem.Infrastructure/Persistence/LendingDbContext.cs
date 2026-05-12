using Microsoft.EntityFrameworkCore;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class LendingDbContext(DbContextOptions<LendingDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderDetailEntity> OrderDetails => Set<OrderDetailEntity>();
    public DbSet<MediaEntity> Media => Set<MediaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.UserId).HasName("users_pkey");
            entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("users_email_key");
            entity.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            // entity.Property(x => x.Nickname).HasColumnName("nickname");
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
            entity.HasKey(x => x.ObjectId).HasName("items_pkey");
            entity.Property(x => x.ObjectId).HasColumnName("object_id").ValueGeneratedOnAdd();
            entity.Property(x => x.ObjectName).HasColumnName("object_name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.CurrentStatus)
                .HasColumnName("current_status")
                .HasMaxLength(50)
                .HasDefaultValue("Available")
                .ValueGeneratedOnAdd();
            entity.Property(x => x.OwnerId).HasColumnName("owner_id");
            entity.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(300).HasDefaultValue(null);
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.OrderId).HasName("orders_pkey");
            entity.Property(x => x.OrderId).HasColumnName("order_id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(x => x.EndTime).HasColumnName("end_time").IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .HasConstraintName("orders_user_id_fkey")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<OrderDetailEntity>(entity =>
        {
            entity.ToTable("order_details");
            entity.HasKey(x => x.ObjectDetailId).HasName("order_details_pkey");
            entity.Property(x => x.ObjectDetailId).HasColumnName("order_detail_id").ValueGeneratedOnAdd();
            entity.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(x => x.ObjectId).HasColumnName("object_id").IsRequired();
            entity.Property(x => x.DetailStatus).HasColumnName("detail_status").HasMaxLength(50).IsRequired();
            entity.Property(x => x.ActualReturnTime).HasColumnName("actual_return_time");
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.OrderId)
                .HasConstraintName("order_details_order_id_fkey")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Item)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.ObjectId)
                .HasConstraintName("order_details_object_id_fkey")
                .OnDelete(DeleteBehavior.Restrict);
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
