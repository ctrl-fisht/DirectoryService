using DirectoryService.Domain;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.EfCore.Configurations;

public class LocationsConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations", tb =>
        {
            // constraints configuration
            tb.HasCheckConstraint(
                "CK_locations_name_format",
                "\"name\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");

            tb.HasCheckConstraint(
                "CK_locations_name_length",
                $"char_length(\"name\") >= {Constants.LocationConstants.NameMinLength} " +
                $"AND char_length(\"name\") <= {Constants.LocationConstants.NameMaxLength}");
            
            tb.HasCheckConstraint(
                "CK_locations_address_country",
                "\"address_country\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");

            tb.HasCheckConstraint(
                "CK_locations_address_city",
                "\"address_city\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");

            tb.HasCheckConstraint(
                "CK_locations_address_street",
                "\"address_street\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");

            tb.HasCheckConstraint(
                "CK_locations_address_building",
                "\"address_building\" ~ '^[A-Za-zА-Яа-яЁё0-9 \\-]+$'");
        });

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id");
        
        
        // properties configuration
        
        builder.ComplexProperty(l => l.LocationName, lnb =>
        {
            lnb.Property(name => name.Value)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(Constants.LocationConstants.NameMaxLength);
        });

        builder.ComplexProperty(l => l.Address, ab =>
        {
            ab.Property(a => a.Country)
                .HasColumnName("address_country");
            ab.Property(a => a.City)
                .HasColumnName("address_city");
            ab.Property(a => a.Street)
                .HasColumnName("address_street");
            ab.Property(a => a.Building)
                .HasColumnName("address_building");
        });

        builder.ComplexProperty(l => l.Timezone, tb =>
        {
            tb.Property(timezone => timezone.Value)
                .HasColumnName("timezone")
                .IsRequired();
        });
        
        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        // relationships configuration
        // one-to-many часть связи Many-To-Many с departments
        builder.HasMany(l => l.DepartmentLocations)
            .WithOne(dl => dl.Location)
            .HasForeignKey(dl => dl.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}