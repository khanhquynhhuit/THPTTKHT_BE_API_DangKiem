using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("NguoiDung")]
[Index("Email", Name = "UQ__NguoiDun__A9D1053420CBFADE", IsUnique = true)]
public partial class NguoiDung
{
    [Key]
    public int MaNguoiDung { get; set; }

    [StringLength(100)]
    public string? HoTen { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? MatKhau { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? VaiTro { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayXoa { get; set; }

    [InverseProperty("MaNhanVienNavigation")]
    public virtual ICollection<KetQuaDangKiem> KetQuaDangKiems { get; set; } = new List<KetQuaDangKiem>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<PhuongTien> PhuongTiens { get; set; } = new List<PhuongTien>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
