using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Datasheet> Datasheets => Set<Datasheet>();
    public DbSet<AlternativeMatch> AlternativeMatches => Set<AlternativeMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Part Configuration ---
        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PartNumber).IsUnique(); // Ensure the part number is unique

            // Setting the AdditionalFeatures column to JSONB on the PostgreSQL side
            entity.Property(e => e.AdditionalFeatures).HasColumnType("jsonb");

            // Manufacturer Relationship
            entity.HasOne(e => e.Manufacturer)
            .WithMany(m => m.Parts)
            .HasForeignKey(e => e.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        // --- AlternativeMatch Configuration ---
        modelBuilder.Entity<AlternativeMatch>(entity =>
        {
            entity.HasKey(e => e.Id);

            // SourcePart Relationship
            entity.HasOne(e => e.SourcePart)
            .WithMany(p => p.SourceMatches)
            .HasForeignKey(e => e.SourcePartId)
            .OnDelete(DeleteBehavior.Restrict);

            // TargetPart Relationship
            entity.HasOne(e => e.TargetPart)
            .WithMany(p => p.TargetMatches)
            .HasForeignKey(e => e.TargetPartId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Datasheet Configuration ---

        modelBuilder.Entity<Datasheet>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Part)
            .WithMany(d => d.Datasheets)
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
