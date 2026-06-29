using Microsoft.AspNetCore.Mvc;
using InventoryManagementApi.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> CreateUserAsync(string email, string password, string roleName)
    {
        if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email and password should not be empty.");

        var user = new ApplicationUser { UserName = email, Email = email };

        // Password is hashed automatically by identity
        var createUserResult = await _userManager.CreateAsync(user, password);

        if(!createUserResult.Succeeded)
            return BadRequest(createUserResult.Errors);

        var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);

        if(!addRoleResult.Succeeded)
            return BadRequest($"Failed to add a role to the user {user.UserName}. Roles should be 'Admin', 'SalesAgent', 'InventoryAgent', 'Customer'.");

        return Ok($"User '{email}' created and assigned to role '{roleName}'.");
    }

    [HttpPost("login")]
    public async Task<IResult> LoginAsync(string email, string password)
        {
            // searches for user by email and returns unauthorized, if not found
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null) 
                return Results.Unauthorized();

            // check for user password and returns unauthorized, if not matching
            if (!await _userManager.CheckPasswordAsync(user, password))
                return Results.Unauthorized();

            var rolesOfUser = await _userManager.GetRolesAsync(user);

            // Create JWT claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // add user roles to claims
            claims.AddRange(rolesOfUser.Select(role => new Claim(ClaimTypes.Role, role)));

            // create a JWT Token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new { token = tokenString });
        }
}