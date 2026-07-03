namespace API_QuanLyDangKiem.DTOs
{
    public class NotificationDto
    {
        public int MaThongBao { get; set; }
        public int? MaPhuongTien { get; set; }
        public string NoiDung { get; set; }
        public string LoaiThongBao { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}
