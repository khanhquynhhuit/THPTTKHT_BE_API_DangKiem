using API_QuanLyDangKiem.DTOs;
using API_QuanLyDangKiem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var history = await _context.LichSuThanhToans
                .Include(x => x.MaThanhToanNavigation)
                .ThenInclude(t => t.MaLichHenNavigation)
                .ThenInclude(l => l.MaPhuongTienNavigation)
                .ThenInclude(p => p.MaNguoiDungNavigation)
                .OrderByDescending(x => x.NgayTao)
                .Select(x => new PaymentHistoryDto
                {
                    MaLichSu = x.MaLichSu,
                    MaThanhToan = x.MaThanhToan ?? 0,
                    SoTien = x.MaThanhToanNavigation != null ? x.MaThanhToanNavigation.SoTien ?? 0 : 0,
                    PhuongThucThanhToan = x.MaThanhToanNavigation != null ? x.MaThanhToanNavigation.PhuongThucThanhToan : "",
                    BienSo = (x.MaThanhToanNavigation != null && x.MaThanhToanNavigation.MaLichHenNavigation != null && x.MaThanhToanNavigation.MaLichHenNavigation.MaPhuongTienNavigation != null) ? x.MaThanhToanNavigation.MaLichHenNavigation.MaPhuongTienNavigation.BienSo : "",
                    TenNguoiDung = (x.MaThanhToanNavigation != null && x.MaThanhToanNavigation.MaLichHenNavigation != null && x.MaThanhToanNavigation.MaLichHenNavigation.MaPhuongTienNavigation != null && x.MaThanhToanNavigation.MaLichHenNavigation.MaPhuongTienNavigation.MaNguoiDungNavigation != null) ? x.MaThanhToanNavigation.MaLichHenNavigation.MaPhuongTienNavigation.MaNguoiDungNavigation.HoTen : "",
                    TrangThai = x.TrangThai,
                    GhiChu = x.GhiChu,
                    NgayTao = x.NgayTao ?? DateTime.Now
                })
                .ToListAsync();

            return Ok(history);
        }
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var payments = await _context.ThanhToans
                .Include(x => x.MaLichHenNavigation)
                .ThenInclude(l => l.MaPhuongTienNavigation)
                .Where(x => x.MaLichHenNavigation.MaPhuongTienNavigation.MaNguoiDung == userId)
                .OrderByDescending(x => x.NgayTao)
                .Select(x => new
                {
                    MaThanhToan = x.MaThanhToan,
                    MaLichHen = x.MaLichHen,
                    BienSo = x.MaLichHenNavigation.MaPhuongTienNavigation.BienSo,
                    SoTien = x.SoTien,
                    TrangThai = x.TrangThai,
                    PhuongThucThanhToan = x.PhuongThucThanhToan,
                    NgayTao = x.NgayTao,
                    NgayThanhToan = x.NgayThanhToan
                })
                .ToListAsync();

            return Ok(payments);
        }

        public class PayRequest {
            public string phuongThuc { get; set; }
        }

        [Authorize]
        [HttpPost("{id}/pay")]
        public async Task<IActionResult> Pay(int id, [FromBody] PayRequest req)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var thanhToan = await _context.ThanhToans
                .Include(x => x.MaLichHenNavigation)
                .ThenInclude(l => l.MaPhuongTienNavigation)
                .FirstOrDefaultAsync(x => x.MaThanhToan == id && x.MaLichHenNavigation.MaPhuongTienNavigation.MaNguoiDung == userId);

            if (thanhToan == null)
                return NotFound("Không tìm thấy hóa đơn");

            if (thanhToan.TrangThai == "SUCCESS")
                return BadRequest("Hóa đơn này đã được thanh toán");

            thanhToan.TrangThai = "SUCCESS";
            thanhToan.PhuongThucThanhToan = req.phuongThuc;
            thanhToan.NgayThanhToan = DateTime.Now;
            thanhToan.MaGiaoDich = "MOCK_" + DateTime.Now.Ticks.ToString();

            var lichSu = new LichSuThanhToan
            {
                MaThanhToan = thanhToan.MaThanhToan,
                TrangThai = "SUCCESS",
                GhiChu = "Thanh toán thành công qua " + req.phuongThuc,
                NgayTao = DateTime.Now
            };
            _context.LichSuThanhToans.Add(lichSu);

            // Cập nhật ngày kiểm định cho xe nếu kết quả là PASS
            var ketQua = await _context.KetQuaDangKiems
                .FirstOrDefaultAsync(k => k.MaLichHen == thanhToan.MaLichHen);
                
            if (ketQua != null && ketQua.KetQua == "PASS")
            {
                var vehicle = thanhToan.MaLichHenNavigation.MaPhuongTienNavigation;
                vehicle.NgayDangKiemGanNhat = DateOnly.FromDateTime(DateTime.Now);
                if (ketQua.NgayDangKiemTiepTheo.HasValue)
                {
                    vehicle.NgayDangKiemTiepTheo = ketQua.NgayDangKiemTiepTheo;
                }
            }

            var thongBao = new ThongBao
            {
                MaNguoiDung = userId,
                MaPhuongTien = thanhToan.MaLichHenNavigation.MaPhuongTien,
                NoiDung = $"Thanh toán thành công hóa đơn đăng kiểm xe {thanhToan.MaLichHenNavigation.MaPhuongTienNavigation.BienSo} số tiền {thanhToan.SoTien} VND.",
                LoaiThongBao = "SYSTEM",
                TrangThai = "SENT",
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Thanh toán thành công" });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("revenue-stats")]
        public async Task<IActionResult> GetRevenueStats()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            // Chỉ tính những thanh toán thành công
            var successfulPayments = await _context.ThanhToans
                .Where(x => x.TrangThai == "SUCCESS" && x.NgayThanhToan.HasValue)
                .Select(x => new { x.SoTien, x.NgayThanhToan })
                .ToListAsync();

            var revenueToday = successfulPayments
                .Where(x => x.NgayThanhToan.Value.Date == today)
                .Sum(x => x.SoTien ?? 0);

            var revenueMonth = successfulPayments
                .Where(x => x.NgayThanhToan.Value >= startOfMonth)
                .Sum(x => x.SoTien ?? 0);

            var revenueYear = successfulPayments
                .Where(x => x.NgayThanhToan.Value >= startOfYear)
                .Sum(x => x.SoTien ?? 0);

            return Ok(new
            {
                today = revenueToday,
                month = revenueMonth,
                year = revenueYear
            });
        }
    }
}
