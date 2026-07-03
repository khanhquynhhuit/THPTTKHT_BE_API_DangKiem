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
    public class VehicleTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicleTypes = await _context.LoaiXes
                .Select(x => new VehicleTypeDto
                {
                    MaLoaiXe = x.MaLoaiXe,
                    TenLoaiXe = x.TenLoaiXe,
                    ChuKyDangKiemThang = x.ChuKyDangKiemThang
                })
                .ToListAsync();

            return Ok(vehicleTypes);
        }


        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicleType = await _context.LoaiXes
                .Where(x => x.MaLoaiXe == id)
                .Select(x => new VehicleTypeDto
                {
                    MaLoaiXe = x.MaLoaiXe,
                    TenLoaiXe = x.TenLoaiXe,
                    ChuKyDangKiemThang = x.ChuKyDangKiemThang
                })
                .FirstOrDefaultAsync();

            if (vehicleType == null)
            {
                return NotFound("Không tìm thấy loại xe");
            }

            return Ok(vehicleType);
        }

        // =========================
        // CREATE
        // =========================
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(
            VehicleTypeCreateDto dto)
        {
            var vehicleType = new LoaiXe
            {
                TenLoaiXe = dto.TenLoaiXe,
                ChuKyDangKiemThang =
                    dto.ChuKyDangKiemThang
            };

            _context.LoaiXes.Add(vehicleType);

            await _context.SaveChangesAsync();

            return Ok(vehicleType);
        }

        // =========================
        // UPDATE
        // =========================
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            VehicleTypeUpdateDto dto)
        {
            var vehicleType = await _context.LoaiXes
                .FirstOrDefaultAsync(x =>
                    x.MaLoaiXe == id);

            if (vehicleType == null)
            {
                return NotFound("Không tìm thấy loại xe");
            }

            vehicleType.TenLoaiXe =
                dto.TenLoaiXe;

            vehicleType.ChuKyDangKiemThang =
                dto.ChuKyDangKiemThang;

            await _context.SaveChangesAsync();

            return Ok(vehicleType);
        }

        // =========================
        // DELETE
        // =========================
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicleType = await _context.LoaiXes
                .FirstOrDefaultAsync(x =>
                    x.MaLoaiXe == id);

            if (vehicleType == null)
            {
                return NotFound("Không tìm thấy loại xe");
            }

            _context.LoaiXes.Remove(vehicleType);

            await _context.SaveChangesAsync();

            return Ok("Xóa thành công");
        }
    }
}
