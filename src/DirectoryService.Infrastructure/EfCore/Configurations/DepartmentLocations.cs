using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.EfCore.Configurations;

public class DepartmentLocations : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations", tb =>
        {

        });
        
        builder.HasKey(dl => new { dl.DepartmentId, dl.LocationId });
        
        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id");
        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id");
    }
}