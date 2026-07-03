using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("PhuongTien")]
[Index("BienSo", Name = "UQ__PhuongTi__F7052EB682F498AA", IsUnique = true)]
public partial class PhuongTien
{
    [Key]
    public int MaPhuongTien { get; set; }

    public int? MaNguoiDung { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? BienSo { get; set; }

    public int? MaLoaiXe { get; set; }

    public int? NamSanXuat { get; set; }

    public DateOnly? NgayDangKiemGanNhat { get; set; }

    public DateOnly? NgayDangKiemTiepTheo { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? TrangThai { get; set; }

    [InverseProperty("MaPhuongTienNavigation")]
    public virtual ICollection<LichHenDangKiem> LichHenDangKiems { get; set; } = new List<LichHenDangKiem>();

    [ForeignKey("MaLoaiXe")]
    [InverseProperty("PhuongTiens")]
    public virtual LoaiXe? MaLoaiXeNavigation { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("PhuongTiens")]
    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    [InverseProperty("MaPhuongTienNavigation")]
    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
