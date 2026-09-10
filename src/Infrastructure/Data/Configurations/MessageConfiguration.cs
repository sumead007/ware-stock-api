using WareStockApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WareStockApi.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(m => m.SenderId).IsRequired();
        builder.Property(m => m.Content).HasMaxLength(4000).IsRequired();

        builder.HasIndex(m => m.ConversationId);
    }
}
