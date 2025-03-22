using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class Phim
{
    public int MaPhim { get; set; }

    public string TenPhim { get; set; } = null!;

    public string TheLoai { get; set; } = null!;

    public int ThoiLuong { get; set; }

    public string? MoTa { get; set; }

    public DateOnly NgayKhoiChieu { get; set; }

    public string? Poster { get; set; }

    public bool? TrangThai { get; set; }

    public string? DoTuoi { get; set; }

    public string? DienVien { get; set; }

    public string? DaoDien { get; set; }

    public string? NgonNgu { get; set; }

    public virtual ICollection<LichChieu> LichChieus { get; set; } = new List<LichChieu>();

    public virtual ICollection<Trailer> Trailers { get; set; } = new List<Trailer>();
}
