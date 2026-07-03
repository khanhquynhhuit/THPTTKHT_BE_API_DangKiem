using System.Security.Claims;
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
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
    VehicleCreateDto dto)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var vehicle = new PhuongTien
            {
                MaNguoiDung = int.Parse(userId),

                BienSo = dto.BienSo,

                MaLoaiXe = dto.MaLoaiXe,

                NamSanXuat = dto.NamSanXuat,

                TrangThai = "ACTIVE"
            };

            _context.PhuongTiens.Add(vehicle);

            await _context.SaveChangesAsync();

            return Ok(vehicle);
        }



        [HttpGet("my")]
        public async Task<IActionResult> MyVehicles()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var vehicles = await _context.PhuongTiens
                .Where(x => x.MaNguoiDung == int.Parse(userId))
                .Select(x => new VehicleResponseDto
                {
                    MaPhuongTien = x.MaPhuongTien,
                    BienSo = x.BienSo,
                    MaLoaiXe = x.MaLoaiXe,
                    NamSanXuat = x.NamSanXuat ?? 0,
                    TrangThai = x.TrangThai,
                    NgayDangKiemGanNhat = x.NgayDangKiemGanNhat,
                    NgayDangKiemTiepTheo = x.NgayDangKiemTiepTheo
                })
                .ToListAsync();

            return Ok(vehicles);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    int id,
    VehicleUpdateDto dto)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var vehicle = await _context.PhuongTiens
                .FirstOrDefaultAsync(x =>
                    x.MaPhuongTien == id &&
                    x.MaNguoiDung == int.Parse(userId));

            if (vehicle == null)
            {
                return NotFound();
            }

            vehicle.BienSo = dto.BienSo;
            vehicle.MaLoaiXe = dto.MaLoaiXe;
            vehicle.NamSanXuat = dto.NamSanXuat;
            vehicle.TrangThai = dto.TrangThai;

            await _context.SaveChangesAsync();

            return Ok(vehicle);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var vehicle = await _context.PhuongTiens
                .FirstOrDefaultAsync(x =>
                    x.MaPhuongTien == id &&
                    x.MaNguoiDung == int.Parse(userId));

            if (vehicle == null)
            {
                return NotFound();
            }

            _context.PhuongTiens.Remove(vehicle);

            await _context.SaveChangesAsync();

            return Ok("Xóa thành công");
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _context.PhuongTiens
                .Include(x => x.MaNguoiDungNavigation)
                .Include(x => x.MaLoaiXeNavigation)
                .Select(x => new
                {
                    maPhuongTien = x.MaPhuongTien,
                    bienSo = x.BienSo,
                    chuXe = x.MaNguoiDungNavigation.HoTen,
                    sdt = x.MaNguoiDungNavigation.SoDienThoai,
                    email = x.MaNguoiDungNavigation.Email,
                    loaiXe = x.MaLoaiXeNavigation != null ? x.MaLoaiXeNavigation.TenLoaiXe : "",
                    namSanXuat = x.NamSanXuat,
                    trangThai = x.TrangThai,
                    ngayDangKiemGanNhat = x.NgayDangKiemGanNhat,
                    ngayDangKiemTiepTheo = x.NgayDangKiemTiepTheo
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        [Authorize(Roles = "STAFF,ADMIN")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchVehicleByLicensePlate([FromQuery] string bienSo)
        {
            if (string.IsNullOrWhiteSpace(bienSo))
                return BadRequest("Vui lòng nhập biển số xe.");

            // Tìm xe kèm thông tin Chủ xe, Loại xe và Lịch sử đăng kiểm
            var vehicle = await _context.PhuongTiens
                .Include(x => x.MaNguoiDungNavigation)
                .Include(x => x.MaLoaiXeNavigation)
                .Include(x => x.LichHenDangKiems)
                    .ThenInclude(lh => lh.MaTrungTamNavigation)
                .Include(x => x.LichHenDangKiems)
                    .ThenInclude(lh => lh.KetQuaDangKiem)
                .FirstOrDefaultAsync(x => x.BienSo.ToLower() == bienSo.ToLower());

            if (vehicle == null)
                return NotFound("Không tìm thấy dữ liệu về phương tiện này.");

            return Ok(new
            {
                phuongTien = new
                {
                    maPhuongTien = vehicle.MaPhuongTien,
                    bienSo = vehicle.BienSo,
                    namSanXuat = vehicle.NamSanXuat,
                    loaiXe = vehicle.MaLoaiXeNavigation?.TenLoaiXe,
                    trangThai = vehicle.TrangThai
                },
                chuXe = new
                {
                    hoTen = vehicle.MaNguoiDungNavigation?.HoTen,
                    soDienThoai = vehicle.MaNguoiDungNavigation?.SoDienThoai,
                    email = vehicle.MaNguoiDungNavigation?.Email
                },
                lichSuKham = vehicle.LichHenDangKiems
                    .OrderByDescending(lh => lh.ThoiGianHen)
                    .Select(lh => new
                    {
                        maLichHen = lh.MaLichHen,
                        ngayKham = lh.ThoiGianHen,
                        trangThaiLich = lh.TrangThai,
                        trungTam = lh.MaTrungTamNavigation?.TenTrungTam,
                        ketQua = lh.KetQuaDangKiem?.KetQua,
                        loiViPham = lh.KetQuaDangKiem?.GhiChu,
                        ngayKhamTiepTheo = lh.KetQuaDangKiem?.NgayDangKiemTiepTheo
                    }).ToList()
            });
        }
    }
}
