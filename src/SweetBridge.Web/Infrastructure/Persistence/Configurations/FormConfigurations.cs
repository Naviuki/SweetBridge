using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="SupportRequest"/>.
/// </summary>
public sealed class SupportRequestConfiguration : IEntityTypeConfiguration<SupportRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SupportRequest> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("SupportRequests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Name).HasMaxLength(200).IsRequired();
        builder.Property(request => request.Email).HasMaxLength(320).IsRequired();
        builder.Property(request => request.OrderNumber).HasMaxLength(16);
        builder.Property(request => request.Message).HasMaxLength(2000).IsRequired();
        builder.HasIndex(request => request.Status);
    }
}

/// <summary>
/// Сопоставление для <see cref="CorporateRequest"/>.
/// </summary>
public sealed class CorporateRequestConfiguration : IEntityTypeConfiguration<CorporateRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CorporateRequest> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("CorporateRequests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Company).HasMaxLength(200).IsRequired();
        builder.Property(request => request.ContactName).HasMaxLength(200).IsRequired();
        builder.Property(request => request.Email).HasMaxLength(320).IsRequired();
        builder.Property(request => request.Volume).HasMaxLength(100).IsRequired();
        builder.Property(request => request.Comment).HasMaxLength(2000);
        builder.HasIndex(request => request.Status);
    }
}

/// <summary>
/// Сопоставление для <see cref="NewsletterSubscriber"/>.
/// </summary>
public sealed class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("NewsletterSubscribers");
        builder.HasKey(subscriber => subscriber.Id);
        builder.Property(subscriber => subscriber.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(subscriber => subscriber.Email).IsUnique();
        builder.Property(subscriber => subscriber.ConfirmationToken).HasMaxLength(64).IsRequired();
        builder.Property(subscriber => subscriber.UnsubscribeToken).HasMaxLength(64).IsRequired();
    }
}

/// <summary>
/// Сопоставление для <see cref="AuditLogEntry"/>.
/// </summary>
public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AuditLogEntries");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.UserId).HasMaxLength(450).IsRequired();
        builder.Property(entry => entry.Action).HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.TargetType).HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.Details).HasMaxLength(1000);
        builder.HasIndex(entry => entry.CreatedAtUtc);
    }
}