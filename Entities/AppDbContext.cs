using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BangGiaDangKiem> BangGiaDangKiems { get; set; }

    public virtual DbSet<KetQuaDangKiem> KetQuaDangKiems { get; set; }

    public virtual DbSet<KhungGio> KhungGios { get; set; }

    public virtual DbSet<LichHenDangKiem> LichHenDangKiems { get; set; }

    public virtual DbSet<LichSuThanhToan> LichSuThanhToans { get; set; }

    public virtual DbSet<LoaiXe> LoaiXes { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<PhuongTien> PhuongTiens { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TrungTamDangKiem> TrungTamDangKiems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-6A1M9L9L\\SQLEXPRESS;Database=QL_DangKiem;User Id=sa;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BangGiaDangKiem>(entity =>
        {
            entity.HasKey(e => e.MaGia).HasName("PK__BangGiaD__3CD3DE5E4E9E7964");

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.BangGiaDangKiems)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__BangGiaDa__MaLoa__3F466844");
        });

        modelBuilder.Entity<KetQuaDangKiem>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("PK__KetQuaDa__D5B3102A077B2D90");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaLichHenNavigation).WithOne(p => p.KetQuaDangKiem)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__KetQuaDan__MaLic__5BE2A6F2");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.KetQuaDangKiems).HasConstraintName("FK__KetQuaDan__MaNha__5CD6CB2B");
        });

        modelBuilder.Entity<KhungGio>(entity =>
        {
            entity.HasKey(e => e.MaKhungGio).HasName("PK__KhungGio__1EC172694BA1CF85");

            entity.Property(e => e.SoLuongDaDat).HasDefaultValue(0);

            entity.HasOne(d => d.MaTrungTamNavigation).WithMany(p => p.KhungGios)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__KhungGio__MaTrun__4D94879B");
        });

        modelBuilder.Entity<LichHenDangKiem>(entity =>
        {
            entity.HasKey(e => e.MaLichHen).HasName("PK__LichHenD__150F264FE74039D9");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("BOOKED");

            entity.HasOne(d => d.MaKhungGioNavigation).WithMany(p => p.LichHenDangKiems).HasConstraintName("FK__LichHenDa__MaKhu__5629CD9C");

            entity.HasOne(d => d.MaPhuongTienNavigation).WithMany(p => p.LichHenDangKiems)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__LichHenDa__MaPhu__5441852A");

            entity.HasOne(d => d.MaTrungTamNavigation).WithMany(p => p.LichHenDangKiems).HasConstraintName("FK__LichHenDa__MaTru__5535A963");
        });

        modelBuilder.Entity<LichSuThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSuTh__C443222A80FCDF07");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaThanhToanNavigation).WithMany(p => p.LichSuThanhToans)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__LichSuTha__MaTha__71D1E811");
        });

        modelBuilder.Entity<LoaiXe>(entity =>
        {
            entity.HasKey(e => e.MaLoaiXe).HasName("PK__LoaiXe__122512B5B6ED1726");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762621E8431");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.VaiTro).HasDefaultValue("USER");
        });

        modelBuilder.Entity<PhuongTien>(entity =>
        {
            entity.HasKey(e => e.MaPhuongTien).HasName("PK__PhuongTi__35B6C8B0E8EDFF95");

            entity.Property(e => e.TrangThai).HasDefaultValue("ACTIVE");

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.PhuongTiens).HasConstraintName("FK__PhuongTie__MaLoa__45F365D3");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.PhuongTiens)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PhuongTie__MaNgu__44FF419A");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__ThanhToa__D4B258444C9A2619");

            entity.Property(e => e.DonViTienTe).HasDefaultValue("VND");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("PENDING");

            entity.HasOne(d => d.MaLichHenNavigation).WithOne(p => p.ThanhToan)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__ThanhToan__MaLic__6D0D32F4");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.MaThongBao).HasName("PK__ThongBao__04DEB54EB4DF73A9");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("SENT");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.ThongBaos)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__ThongBao__MaNguo__6383C8BA");

            entity.HasOne(d => d.MaPhuongTienNavigation).WithMany(p => p.ThongBaos).HasConstraintName("FK__ThongBao__MaPhuo__6477ECF3");
        });

        modelBuilder.Entity<TrungTamDangKiem>(entity =>
        {
            entity.HasKey(e => e.MaTrungTam).HasName("PK__TrungTam__54A2B84FE743F1E8");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
