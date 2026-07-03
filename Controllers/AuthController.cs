using API_QuanLyDangKiem.DTOs;
using Microsoft.EntityFrameworkCore;
using API_QuanLyDangKiem.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace API_QuanLyDangKiem.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //private readonly AppDbContext _context;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        //public AuthController(AppDbContext context)
        //{
        //    _context = context;
        //}
        public AuthController(AppDbContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (_context.NguoiDungs.Any(x => x.Email == dto.Email))
            {
                return BadRequest("Email đã tồn tại");
            }

            var user = new NguoiDung
            {
                HoTen = dto.HoTen,
                Email = dto.Email,
                SoDienThoai = dto.SoDienThoai,

                MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),

                VaiTro = "USER",
                NgayTao = DateTime.Now
            };

            _context.NguoiDungs.Add(user);

            await _context.SaveChangesAsync();

            return Ok("Đăng ký thành công");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                return BadRequest("Email không tồn tại");
            }

            if (user.NgayXoa != null)
            {
                return BadRequest("Tài khoản của bạn đã bị vô hiệu hóa");
            }

            bool checkPassword = BCrypt.Net.BCrypt.Verify(dto.MatKhau,user.MatKhau);

            if (!checkPassword)
            {
                return BadRequest("Sai mật khẩu");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.MaNguoiDung.ToString()),

                new Claim(ClaimTypes.Email,user.Email),

                new Claim(ClaimTypes.Role,user.VaiTro)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            if (user.VaiTro == "USER")
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var warningDate = today.AddDays(7);

                var vehicles = await _context.PhuongTiens
                    .Where(v => v.MaNguoiDung == user.MaNguoiDung && v.TrangThai == "ACTIVE" && v.NgayDangKiemTiepTheo != null && v.NgayDangKiemTiepTheo <= warningDate && v.NgayDangKiemTiepTheo >= today)
                    .ToListAsync();

                foreach (var v in vehicles)
                {
                    var message = $"Xe biển số {v.BienSo} sắp đến hạn đăng kiểm vào ngày {v.NgayDangKiemTiepTheo?.ToString("dd/MM/yyyy")}. Quý khách vui lòng đặt lịch!";
                    var existingNotif = await _context.ThongBaos
                        .AnyAsync(t => t.MaNguoiDung == user.MaNguoiDung && t.MaPhuongTien == v.MaPhuongTien && t.NoiDung == message);

                    if (!existingNotif)
                    {
                        var notif = new ThongBao
                        {
                            MaNguoiDung = user.MaNguoiDung,
                            MaPhuongTien = v.MaPhuongTien,
                            NoiDung = message,
                            LoaiThongBao = "REMINDER",
                            TrangThai = "UNREAD",
                            NgayTao = DateTime.Now
                        };
                        _context.ThongBaos.Add(notif);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                token = jwt,

                user = new
                {
                    id = user.MaNguoiDung,
                    email = user.Email,
                    role = user.VaiTro
                }
            });
        }



        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = _context.NguoiDungs.FirstOrDefault(x => x.MaNguoiDung == userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                id = user.MaNguoiDung,
                hoTen = user.HoTen,
                email = user.Email,
                soDienThoai = user.SoDienThoai,
                vaiTro = user.VaiTro
            });
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.NguoiDungs
                .Where(u => u.NgayXoa == null)
                .Select(u => new
                {
                    id = u.MaNguoiDung,
                    hoTen = u.HoTen,
                    email = u.Email,
                    soDienThoai = u.SoDienThoai,
                    vaiTro = u.VaiTro,
                    ngayTao = u.NgayTao
                })
                .ToListAsync();

            return Ok(users);
        }

        public class ChangeRoleRequest {
            public string role { get; set; }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("change-role/{id}")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest req)
        {
            var newRole = req.role;
            if (newRole != "USER" && newRole != "STAFF" && newRole != "ADMIN")
                return BadRequest("Vai trò không hợp lệ");

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
                return NotFound("Không tìm thấy người dùng");

            if (user.VaiTro == "ADMIN")
                return BadRequest("Không thể thay đổi vai trò của Admin!");

            user.VaiTro = newRole;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật vai trò thành công" });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null || user.NgayXoa != null)
                return NotFound("Không tìm thấy người dùng");

            if (user.VaiTro == "ADMIN")
                return BadRequest("Không thể xóa tài khoản Admin!");

            // Xóa mềm bằng cách gán NgayXoa
            user.NgayXoa = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa người dùng thành công" });
        }
    }
}
