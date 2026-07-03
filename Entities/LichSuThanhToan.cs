using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("LichSuThanhToan")]
public partial class LichSuThanhToan
{
    [Key]
    public int MaLichSu { get; set; }

    public int? MaThanhToan { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [ForeignKey("MaThanhToan")]
    [InverseProperty("LichSuThanhToans")]
    public virtual ThanhToan? MaThanhToanNavigation { get; set; }
}
