using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("ThongBao")]
public partial class ThongBao
{
    [Key]
    public int MaThongBao { get; set; }

    public int? MaNguoiDung { get; set; }

    public int? MaPhuongTien { get; set; }

    public string? NoiDung { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? LoaiThongBao { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("ThongBaos")]
    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    [ForeignKey("MaPhuongTien")]
    [InverseProperty("ThongBaos")]
    public virtual PhuongTien? MaPhuongTienNavigation { get; set; }
}
