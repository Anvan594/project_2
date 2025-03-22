using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebBanVeXemPhim.Models;

public partial class QuanLyBanVeXemPhimContext : DbContext
{
    public QuanLyBanVeXemPhimContext()
    {
    }

    public QuanLyBanVeXemPhimContext(DbContextOptions<QuanLyBanVeXemPhimContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<Combo> Comboes { get; set; }

    public virtual DbSet<Ghe> Ghes { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LichChieu> LichChieus { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<Phim> Phims { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<TaiKhoanAdmin> TaiKhoanAdmins { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<Trailer> Trailers { get; set; }

    public virtual DbSet<Ve> Ves { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.

        => optionsBuilder.UseSqlServer("Server=DESKTOP-GHP029P\\SQLEXPRESS;Database=QuanLyBanVeXemPhim_v2;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.MaBanner).HasName("PK__Banner__508B4A497445066A");

            entity.ToTable("Banner");

            entity.Property(e => e.MoTa)
                .HasMaxLength(50)
                .IsFixedLength();
        });

        modelBuilder.Entity<Combo>(entity =>
        {
            entity.HasKey(e => e.MaCombo).HasName("PK__Combo__C3EDBC780EC4547C");

            entity.Property(e => e.Anh).HasMaxLength(50);
            entity.Property(e => e.Gia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TenCombo).HasMaxLength(50);
        });

        modelBuilder.Entity<Ghe>(entity =>
        {
            entity.HasKey(e => e.MaGhe).HasName("PK__Ghe__3CD3C67B277838C9");

            entity.ToTable("Ghe");

            entity.Property(e => e.LoaiGhe).HasMaxLength(50);
            entity.Property(e => e.SoGhe).HasMaxLength(10);
            entity.Property(e => e.TrangThai).HasDefaultValue(false);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Ghes)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ghe__MaPhong__3B75D760");
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaKhuyenMai).HasName("PK__KhuyenMa__6F56B3BD22D6A6E3");

            entity.ToTable("KhuyenMai");
        });

        modelBuilder.Entity<LichChieu>(entity =>
        {
            entity.HasKey(e => e.MaLichChieu).HasName("PK__LichChie__DC7401970F7079DB");

            entity.ToTable("LichChieu");

            entity.Property(e => e.GiaVe).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaPhimNavigation).WithMany(p => p.LichChieus)
                .HasForeignKey(d => d.MaPhim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichChieu__MaPhi__3F466844");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.LichChieus)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichChieu__MaPho__403A8C7D");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762F1F5E2ED");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D105348E78FD85").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.TenNguoiDung).HasMaxLength(100);
            entity.Property(e => e.Token)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<Phim>(entity =>
        {
            entity.HasKey(e => e.MaPhim).HasName("PK__Phim__4AC03DE3D3E7F109");

            entity.ToTable("Phim");

            entity.Property(e => e.DaoDien)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.DoTuoi)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.NgonNgu)
                .HasMaxLength(20)
                .IsFixedLength();
            entity.Property(e => e.Poster).HasMaxLength(255);
            entity.Property(e => e.TenPhim).HasMaxLength(255);
            entity.Property(e => e.TheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__Phong__20BD5E5B2F6B1B39");

            entity.ToTable("Phong");

            entity.Property(e => e.TenPhong).HasMaxLength(100);
        });

        modelBuilder.Entity<TaiKhoanAdmin>(entity =>
        {
            entity.HasKey(e => e.MaAdmin).HasName("PK__TaiKhoan__49341E38D7FAB88E");

            entity.ToTable("TaiKhoanAdmin");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC003C4EBA0").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D10534278C91B6").IsUnique();

            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenDangNhap).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__ThanhToa__D4B2584488FAFA3F");

            entity.ToTable("ThanhToan");

            entity.Property(e => e.NgayThanhToan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThuc).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.MaVeNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaVe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThanhToan__MaVe__5165187F");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.MaThongBao).HasName("PK__ThongBao__04DEB54EC02B2B02");

            entity.ToTable("ThongBao");

            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThongBao__MaNguo__5535A963");
        });

        modelBuilder.Entity<Trailer>(entity =>
        {
            entity.HasKey(e => e.MaTrailer).HasName("PK__Trailer__E910146543B76BE6");

            entity.ToTable("Trailer");

            entity.Property(e => e.DuongDanTrailer).HasMaxLength(255);
            entity.Property(e => e.MoTaTrailer).HasMaxLength(1000);

            entity.HasOne(d => d.MaPhimNavigation).WithMany(p => p.Trailers)
                .HasForeignKey(d => d.MaPhim)
                .HasConstraintName("FK__Trailer__MaPhim__619B8048");
        });

        modelBuilder.Entity<Ve>(entity =>
        {
            entity.HasKey(e => e.MaVe).HasName("PK__Ve__2725100FE1551889");

            entity.ToTable("Ve");

            entity.Property(e => e.GiaVe).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NgayDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaGheNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaGhe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaGhe__4CA06362");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaKhachHang__4D94879B");

            entity.HasOne(d => d.MaLichChieuNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaLichChieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaLichChieu__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
