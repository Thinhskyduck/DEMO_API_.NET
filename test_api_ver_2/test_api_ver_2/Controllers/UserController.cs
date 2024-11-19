using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_api_ver_2.Models;
using System.Security.Cryptography;
using System.Text;

namespace test_api_ver_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _dbContext;

        public UserController(UserContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (_dbContext.User == null)
            {
                return NotFound();
            }
            return await _dbContext.User.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (_dbContext.User == null)
            {
                return NotFound();
            }
            var user = await _dbContext.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // POST: api/Users/Register
        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(User user)
        {
            if (_dbContext.User.Any(u => u.phone == user.phone))
            {
                return Conflict("Phone already exists");
            }

            // Hash the password before saving
            user.Password = HashPassword(user.Password);

            _dbContext.User.Add(user);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // POST: api/Users/Login
        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginUser(User loginRequest)
        {
            var user = await _dbContext.User
                .FirstOrDefaultAsync(u => u.phone == loginRequest.phone);

            if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
            {
                return Unauthorized("Invalid phone or password");
            }

            return Ok("Login successful");
        }

        // Helper method to hash the password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        // Helper method to verify password
        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            var enteredPasswordHash = HashPassword(enteredPassword);
            return enteredPasswordHash == storedPasswordHash;
        }

        // POST: api/Users/ChangePassword
        [HttpPost("ChangePassword/{id}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePassword request)
        {
            // Kiểm tra xem user có tồn tại không
            var user = await _dbContext.User.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Kiểm tra mật khẩu cũ
            if (!VerifyPassword(request.OldPassword, user.Password))
            {
                return BadRequest(new { message = "Old password is incorrect" });
                
            }

            // Cập nhật mật khẩu mới
            user.Password = HashPassword(request.NewPassword);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating password", error = ex.Message });
            }

            return Ok(new { message = "Password updated successfully" });
        }


        // DELETE: api/Users/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_dbContext.User == null)
            {
                return NotFound();
            }
            var user = await _dbContext.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _dbContext.User.Remove(user);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        private bool UserExist(long id)
        {
            return (_dbContext.User?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // POST: api/Users/AddUser
        [HttpPost("AddUser")]
        public async Task<ActionResult<User>> AddUser([FromBody] User newUser)
        {
            if (_dbContext.User.Any(u => u.phone == newUser.phone))
            {
                return Conflict("Phone already exists");
            }

            newUser.Password = HashPassword(newUser.Password);

            _dbContext.User.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
        }

        // PUT: api/Users/UpdateUser/{id}
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            var existingUser = await _dbContext.User.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound(new { message = "User not found" });
            }
            existingUser.Name = updatedUser.Name;
            existingUser.phone = updatedUser.phone;
            existingUser.Email = updatedUser.Email;
 
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating user", error = ex.Message });
            }

            return Ok(new { message = "User updated successfully" });
        }
    }
}
