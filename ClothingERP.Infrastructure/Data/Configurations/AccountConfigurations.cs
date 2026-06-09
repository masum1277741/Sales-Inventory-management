namespace ClothingERP.Infrastructure.Data.Configurations;

public class AccountTransactionConfiguration : IEntityTypeConfiguration<AccountTransaction>
{
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder.ToTable("AccountTransactions");
        builder.Property(x => x.TransactionNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => x.TransactionNumber).IsUnique();
        builder.HasIndex(x => x.TransactionDate);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");


        builder.Property(x => x.ActionType).IsRequired().HasMaxLength(50);


        builder.Property(x => x.EntityName).IsRequired().HasMaxLength(100);

  
        builder.Property(x => x.EntityId);

        builder.Property(x => x.UserName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(x => x.NewValues).HasColumnType("nvarchar(max)");
        builder.Property(x => x.IPAddress).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(500);

 

        builder.HasIndex(x => x.UserId);


        builder.HasIndex(x => x.CreatedAt);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}