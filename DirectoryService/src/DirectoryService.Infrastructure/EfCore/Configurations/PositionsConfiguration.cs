using DirectoryService.Domain;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.EfCore.Configurations;

public class PositionsConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions", tb =>
        {
            // constraints configurations
            tb.HasCheckConstraint(
                "CK_positions_name_length",
                $"char_length(\"name\") >= {Constants.PositionConstants.NameMinLength} " +
                $"AND char_length(\"name\") <= {Constants.PositionConstants.NameMaxLength}");
           
            tb.HasCheckConstraint(
                "CK_positions_name_format",
                "\"name\" ~ '^[A-Za-zА-Яа-яЁё\\s.-]+$'");
            
            tb.HasCheckConstraint(
                "CK_positions_description_length",
                $"description IS NULL OR char_length(description) <= {Constants.PositionConstants.DescriptionMaxLength}");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id");
        
        // properties configurations
        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Constants.PositionConstants.NameMaxLength);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(Constants.PositionConstants.DescriptionMaxLength);
        
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
        builder.HasMany(p => p.DepartmentPositions)
            .WithOne(dp => dp.Position)
            .HasForeignKey(dp => dp.PositionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}