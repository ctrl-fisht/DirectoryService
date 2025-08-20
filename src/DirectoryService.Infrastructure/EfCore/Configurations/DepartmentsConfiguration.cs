using DirectoryService.Domain;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.EfCore.Configurations;

public class DepartmentsConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments", tb =>
        {
            // constraints configuration
            tb.HasCheckConstraint(
                "CK_departments_name_cyrillic",
                "\"name\" ~ '^[А-Яа-яЁё -]+$'");

            tb.HasCheckConstraint(
                "CK_departments_name_length",
                $"char_length(\"name\") >= {Constants.DepartmentConstants.NameMinLength} " +
                $"AND char_length(\"name\") <= {Constants.DepartmentConstants.NameMaxLength}");
            
            
            
            tb.HasCheckConstraint(
                "CK_departments_identifier_format",
                "\"identifier\" ~ '^[A-Za-z-]+$'");

            tb.HasCheckConstraint(
                "CK_departments_identifier_length",
                $"char_length(\"identifier\") >= {Constants.DepartmentConstants.IdentifierMinLength} " +
                $"AND char_length(\"identifier\") <= {Constants.DepartmentConstants.IdentifierMaxLength}");
            
            
            tb.HasCheckConstraint(
                "CK_departments_path",
                "\"path\" ~ '^(?=.*[A-Za-z])[A-Za-z.-]+$'");
        });
        
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id");
        
        
        // properties configuration
        
        builder.ComplexProperty(d => d.DepartmentName, dnb =>
        {
            dnb.Property(name => name.Value)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(Constants.DepartmentConstants.NameMaxLength);

        });

        builder.ComplexProperty(d => d.Identifier, dib =>
        {
            dib.Property(identifier => identifier.Value)
                .HasColumnName("identifier")
                .IsRequired()
                .HasMaxLength(Constants.DepartmentConstants.IdentifierMaxLength);
        });

        builder.ComplexProperty(d => d.Path, dpb =>
        {
            dpb.Property(p => p.Value)
                .HasColumnName("path")
                .IsRequired();
        });

        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id");
        
        builder.Property(d => d.Depth)
            .HasColumnName("depth")
            .IsRequired();
        
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
        
        // древовидная структура (self-referencing relationship)
        builder.HasOne(d => d.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // one-to-many часть связи Many-To-Many с locations
        builder.HasMany(d => d.DepartmentLocations)
            .WithOne(dl => dl.Department)
            .HasForeignKey(dl => dl.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // one-to-many часть связи Many-To-Many с positions
        builder.HasMany(d => d.DepartmentPositions)
            .WithOne(dp => dp.Department)
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}   