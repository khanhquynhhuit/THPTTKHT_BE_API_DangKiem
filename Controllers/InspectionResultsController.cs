using API_QuanLyDangKiem.DTOs;
using API_QuanLyDangKiem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspectionResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InspectionResultsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "STAFF")]
        [HttpPost]
        public async Task<IActionResult> Create(InspectionResultCreateDto dto)
        {
            var booking = await _context.LichHenDangKiems
                .FirstOrDefaultAsync(x => x.MaLichHen == dto.MaLichHen);

            if (booking == null)
                return NotFound();

            if (booking.TrangThai != "BOOKED")
                return BadRequest("Lịch hẹn không ở trạng thái hợp lệ để ghi nhận kết quả.");

            bool resultExists = await _context.KetQuaDangKiems.AnyAsync(x => x.MaLichHen == dto.MaLichHen);
            if (resultExists)
                return BadRequest("Lịch hẹn này đã được ghi nhận kết quả.");

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var result = new KetQuaDangKiem
            {
                MaLichHen = dto.MaLichHen,
                MaNhanVien = userId,
                KetQua = dto.KetQua,
                GhiChu = dto.GhiChu
            };

            booking.TrangThai = "COMPLETED";

            // lấy xe và loại xe để tính chu kỳ
            var vehicle = await _context.PhuongTiens
                .Include(x => x.MaLoaiXeNavigation)
                .FirstOrDefaultAsync(x => x.MaPhuongTien == booking.MaPhuongTien);

            if (dto.KetQua == "PASS" && vehicle != null)
            {
                int chuKyThang = vehicle.MaLoaiXeNavigation?.ChuKyDangKiemThang ?? 12;
                var nextDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(chuKyThang));

                // Lưu ngày tiếp theo vào bảng kết quả đăng kiểm theo đúng DB
                // Phương tiện sẽ được cập nhật chính thức sau khi thanh toán thành công
                result.NgayDangKiemTiepTheo = nextDate;
            }

            _context.KetQuaDangKiems.Add(result);

            // Tạo hóa đơn thanh toán
            var bangGia = await _context.BangGiaDangKiems.FirstOrDefaultAsync(x => x.MaLoaiXe == vehicle.MaLoaiXe);
            decimal soTien = bangGia?.GiaDangKiem ?? 0;

            var thanhToan = new ThanhToan
            {
                MaLichHen = dto.MaLichHen,
                SoTien = soTien,
                TrangThai = "PENDING",
                NgayTao = DateTime.Now
            };
            _context.ThanhToans.Add(thanhToan);

            // Gửi thông báo cho User
            var thongBao = new ThongBao
            {
                MaNguoiDung = vehicle.MaNguoiDung,
                MaPhuongTien = vehicle.MaPhuongTien,
                NoiDung = $"Xe {vehicle.BienSo} đã có kết quả kiểm định: {(dto.KetQua == "PASS" ? "ĐẠT" : "KHÔNG ĐẠT")}. Vui lòng thanh toán hóa đơn ({soTien} VND).",
                LoaiThongBao = "RESULT",
                TrangThai = "SENT",
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Ghi nhận kết quả thành công",
                maKetQua = result.MaKetQua,
                ketQua = result.KetQua
            });
        }
    }
}
