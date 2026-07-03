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
    public class InspectionCentersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InspectionCentersController(AppDbContext context)
        {
            _context = context;
        }
        // =========================
        // GET ALL (PUBLIC)
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var centers = await _context.TrungTamDangKiems
                .Select(x => new InspectionCenterDto
                {
                    MaTrungTam = x.MaTrungTam,
                    TenTrungTam = x.TenTrungTam,
                    DiaChi = x.DiaChi,
                    GioLamViec = x.GioLamViec
                })
                .ToListAsync();

            return Ok(centers);
        }

        // =========================
        // GET BY ID (PUBLIC)
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var center = await _context.TrungTamDangKiems
                .Where(x => x.MaTrungTam == id)
                .Select(x => new InspectionCenterDto
                {
                    MaTrungTam = x.MaTrungTam,
                    TenTrungTam = x.TenTrungTam,
                    DiaChi = x.DiaChi,
                    GioLamViec = x.GioLamViec
                })
                .FirstOrDefaultAsync();

            if (center == null)
                return NotFound("Không tìm thấy trung tâm");

            return Ok(center);
        }

        // =========================
        // CREATE (ADMIN)
        // =========================
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(InspectionCenterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenTrungTam))
                return BadRequest("Tên trung tâm không hợp lệ");

            var center = new TrungTamDangKiem
            {
                TenTrungTam = dto.TenTrungTam,
                DiaChi = dto.DiaChi,
                GioLamViec = dto.GioLamViec,
                NgayTao = DateTime.Now
            };

            _context.TrungTamDangKiems.Add(center);
            await _context.SaveChangesAsync();

            return Ok(center);
        }

        // =========================
        // UPDATE (ADMIN)
        // =========================
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, InspectionCenterUpdateDto dto)
        {
            var center = await _context.TrungTamDangKiems
                .FirstOrDefaultAsync(x => x.MaTrungTam == id);

            if (center == null)
                return NotFound("Không tìm thấy trung tâm");

            center.TenTrungTam = dto.TenTrungTam;
            center.DiaChi = dto.DiaChi;
            center.GioLamViec = dto.GioLamViec;

            await _context.SaveChangesAsync();

            return Ok(center);
        }

        // =========================
        // DELETE (ADMIN)
        // =========================
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var center = await _context.TrungTamDangKiems
                .FirstOrDefaultAsync(x => x.MaTrungTam == id);

            if (center == null)
                return NotFound("Không tìm thấy trung tâm");

            _context.TrungTamDangKiems.Remove(center);
            await _context.SaveChangesAsync();

            return Ok("Xóa thành công");
        }
    }
}
