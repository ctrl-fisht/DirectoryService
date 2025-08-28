using DirectoryService.Domain;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
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
        });
        
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id");
        
        
        // properties configuration

        builder.Property(d => d.DepartmentName)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Constants.DepartmentConstants.NameMaxLength)
            .HasConversion(
                value => value.Value,
                value => DepartmentName.CreateFromDb(value));
       
        builder.Property(d => d.Identifier)
            .HasColumnName("identifier")
            .IsRequired()
            .HasMaxLength(Constants.DepartmentConstants.IdentifierMaxLength)
            .HasConversion(
                value => value.Value,
                value => Identifier.CreateFromDb(value));

        builder.Property(d => d.Path)
            .HasColumnName("path")
            .IsRequired()
            .HasColumnType("ltree")
            .HasConversion(
                value => value.Value,
                value => DepartmentPath.CreateFromDb(value));

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