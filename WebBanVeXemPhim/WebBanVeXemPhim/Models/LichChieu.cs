using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBanVeXemPhim.Models;

public partial class LichChieu
{
    public int MaLichChieu { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn phim.")]
    public int MaPhim { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn phòng.")]
    public int MaPhong { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn ngày chiếu.")]
    public DateOnly NgayChieu { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn giờ chiếu.")]
    public TimeOnly GioChieu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá vé.")]
    public decimal GiaVe { get; set; }

    public bool? TrangThai { get; set; }

    public virtual Phim? MaPhimNavigation { get; set; } = null!;

    public virtual Phong? MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
