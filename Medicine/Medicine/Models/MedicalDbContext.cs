using Medicine.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

public class MedicalDbContext : IdentityDbContext<ApplicationUser>
{
    public MedicalDbContext(DbContextOptions<MedicalDbContext> options) : base(options) { }

    public DbSet<Drug> Drugs { get; set; }
    public DbSet<Disease> Diseases { get; set; }
    public DbSet<Indication> Indications { get; set; }
    public DbSet<Contraindication> Contraindications { get; set; }
    public DbSet<DrugInteraction> DrugInteractions { get; set; }
    public DbSet<DrugCategory> DrugCategories { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }
    public DbSet<SuggestedProtocol> SuggestedProtocols { get; set; }
    public DbSet<SuggestedProtocolDetail> SuggestedProtocolDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Khóa chính (Composite Keys) cho các bảng trung gian
        modelBuilder.Entity<Indication>().HasKey(i => new { i.DrugId, i.DiseaseId });
        modelBuilder.Entity<Contraindication>().HasKey(c => new { c.DrugId, c.DiseaseId });
        modelBuilder.Entity<DrugInteraction>().HasKey(di => new { di.SourceDrugId, di.TargetDrugId });

        // Cấu hình quan hệ self-referencing cho Drug Interaction (Thuốc - Thuốc)
        modelBuilder.Entity<DrugInteraction>()
            .HasOne(di => di.SourceDrug)
            .WithMany(d => d.InteractionsAsSource)
            .HasForeignKey(di => di.SourceDrugId)
            .OnDelete(DeleteBehavior.Restrict); // Tránh lỗi cascade delete multiple paths

        modelBuilder.Entity<DrugInteraction>()
            .HasOne(di => di.TargetDrug)
            .WithMany(d => d.InteractionsAsTarget)
            .HasForeignKey(di => di.TargetDrugId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình quan hệ Prescription - PrescriptionDetail
        modelBuilder.Entity<PrescriptionDetail>()
            .HasOne(pd => pd.Prescription)
            .WithMany(p => p.PrescriptionDetails)
            .HasForeignKey(pd => pd.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa đơn => xóa chi tiết

        modelBuilder.Entity<PrescriptionDetail>()
            .HasOne(pd => pd.Drug)
            .WithMany()
            .HasForeignKey(pd => pd.DrugId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tạo index cho các trường tìm kiếm thường xuyên
        modelBuilder.Entity<Prescription>()
            .HasIndex(p => p.PrescriptionCode)
            .IsUnique();

        modelBuilder.Entity<Prescription>()
            .HasIndex(p => p.CreatedDate);
    }
}