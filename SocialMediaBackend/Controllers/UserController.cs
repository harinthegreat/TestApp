using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaBackend.DTOs;
using SocialMediaBackend.Models;
using SocialMediaBackend.Services;
using System.Threading.Tasks;

namespace SocialMediaBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;

        public UserController(UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO request)
        {
            var existingUser = await _userService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest(new { error = "Email already exists!" });
            var existingUserWithSameName = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingUserWithSameName != null)
                return BadRequest(new { error = "Username already exists!" });
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = request.Password,
                Role = request.Role
            };

            await _userService.CreateUserAsync(newUser);
            return Created("api/auth/register", new { message = "User registered successfully! Please check your email to verify your account." });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var isVerified = await _userService.VerifyEmailAsync(token);
            if (!isVerified)
                return BadRequest(new { error = "Invalid or expired token." });

            return Ok(new { message = "Email verified successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            var isValid = await _userService.ValidateUserAsync(request.EmailOrUsername, request.Password);
            if (!isValid)
                return Unauthorized(new { error = "Invalid credentials!" });

            var user = await _userService.GetUserByEmailAsync(request.EmailOrUsername) ??
                       await _userService.GetUserByUsernameAsync(request.EmailOrUsername);

            if (user!.IsMfaEnabled)
            {
                var mfaToken = _jwtService.GenerateToken(user, expiresInMinutes: 5);
                return Ok(new { mfaRequired = true, mfaToken });
            }

            var token = _jwtService.GenerateToken(user,5);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _userService.SaveRefreshTokenAsync(user.Id, refreshToken);

            return Ok(new { token, refreshToken, role = user.Role });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminEndpoint()
        {
            return Ok(new { message = "This is an admin-only endpoint" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO request)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
                return Unauthorized(new { error = "Invalid token!" });

            var userId = int.Parse(principal.FindFirst("UserId")!.Value);
            var savedRefreshToken = await _userService.GetRefreshTokenAsync(userId);

            if (savedRefreshToken != request.RefreshToken)
                return Unauthorized(new { error = "Invalid refresh token!" });

            var newJwtToken = _jwtService.GenerateToken(new User { Id = userId },5);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            await _userService.SaveRefreshTokenAsync(userId, newRefreshToken);

            return Ok(new { token = newJwtToken, refreshToken = newRefreshToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDTO request)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
                return Unauthorized(new { error = "Invalid token!" });

            var userId = int.Parse(principal.FindFirst("UserId")!.Value);
            await _userService.RevokeRefreshTokenAsync(userId);

            return Ok(new { message = "Logout successful!" });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found!" });

            return Ok(new UserDTO
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });
        }

        [Authorize]
        [HttpPost("mfa/enable")]
        public async Task<IActionResult> EnableMfa()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found!" });

            var response = await _userService.EnableMfaAsync(user);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("mfa/verify-setup")]
        public async Task<IActionResult> VerifyMfaSetup([FromBody] VerifyMfaRequest request)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found!" });

            bool isValid = await _userService.VerifyMfaSetupAsync(user, request.Code);
            if (!isValid)
                return BadRequest(new { error = "Invalid code!" });

            return Ok(new { message = "MFA enabled successfully!" });
        }

        [HttpPost("mfa/verify-login")]
        public async Task<IActionResult> VerifyMfaLogin([FromBody] VerifyMfaRequest request)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found!" });

            bool isValid = await _userService.VerifyMfaLoginAsync(user, request.Code);
            if (!isValid)
                return Unauthorized(new { error = "Invalid code!" });

            return Ok(new { message = "MFA verified successfully!" });
        }

    }
}