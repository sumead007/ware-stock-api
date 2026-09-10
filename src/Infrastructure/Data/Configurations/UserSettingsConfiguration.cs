using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WareStockApi.Infrastructure.Data.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(s => s.UserId);

        builder.Property(s => s.UserId).ValueGeneratedNever();

        builder.Property(s => s.DisplayItems)
            .HasConversion(
                items => string.Join(',', items),
                csv => string.IsNullOrWhiteSpace(csv)
                    ? new List<SidebarItem>()
                    : csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Enum.Parse<SidebarItem>)
                        .ToList())
            .Metadata.SetValueComparer(new ValueComparer<IList<SidebarItem>>(
                (a, b) => a!.SequenceEqual(b!),
                v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                v => v.ToList()));
    }
}
