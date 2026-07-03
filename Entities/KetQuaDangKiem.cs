using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("KetQuaDangKiem")]
[Index("MaLichHen", Name = "UQ__KetQuaDa__150F264E36C01B17", IsUnique = true)]
public partial class KetQuaDangKiem
{
    [Key]
    public int MaKetQua { get; set; }

    public int? MaLichHen { get; set; }

    public int? MaNhanVien { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? KetQua { get; set; }

    public DateOnly? NgayDangKiemTiepTheo { get; set; }

    public string? GhiChu { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [ForeignKey("MaLichHen")]
    [InverseProperty("KetQuaDangKiem")]
    public virtual LichHenDangKiem? MaLichHenNavigation { get; set; }

    [ForeignKey("MaNhanVien")]
    [InverseProperty("KetQuaDangKiems")]
    public virtual NguoiDung? MaNhanVienNavigation { get; set; }
}
