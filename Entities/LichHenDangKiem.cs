using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("LichHenDangKiem")]
[Index("MaPhuongTien", "ThoiGianHen", Name = "UQ_LichHen", IsUnique = true)]
public partial class LichHenDangKiem
{
    [Key]
    public int MaLichHen { get; set; }

    public int? MaPhuongTien { get; set; }

    public int? MaTrungTam { get; set; }

    public int? MaKhungGio { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGianHen { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [InverseProperty("MaLichHenNavigation")]
    public virtual KetQuaDangKiem? KetQuaDangKiem { get; set; }

    [ForeignKey("MaKhungGio")]
    [InverseProperty("LichHenDangKiems")]
    public virtual KhungGio? MaKhungGioNavigation { get; set; }

    [ForeignKey("MaPhuongTien")]
    [InverseProperty("LichHenDangKiems")]
    public virtual PhuongTien? MaPhuongTienNavigation { get; set; }

    [ForeignKey("MaTrungTam")]
    [InverseProperty("LichHenDangKiems")]
    public virtual TrungTamDangKiem? MaTrungTamNavigation { get; set; }

    [InverseProperty("MaLichHenNavigation")]
    public virtual ThanhToan? ThanhToan { get; set; }
}
