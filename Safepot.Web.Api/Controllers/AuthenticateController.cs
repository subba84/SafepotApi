using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Safepot.Contracts;
using Safepot.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly ISfpDataRepository<SfpUser> _userManager;
        private readonly ISfpDataRepository<SfpUserRoleMap> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(ISfpDataRepository<SfpUser> userManager,
            ISfpDataRepository<SfpUserRoleMap> roleManager,
            IConfiguration configuration)
        {
            this._userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.GetAsync();
            if(user != null)
            {
                user = user.Where(x => x.Mobile == model.Username && x.Password == model.Password);
            }
            if (user != null)
            {
                var userDetails = user.First();
                var userRoles = await _roleManager.GetAsync(x=>x.UserId == userDetails.Id);
                var userRole = (userRoles == null || userRoles.Count() == 0) ? new SfpUserRoleMap() : userRoles.First();
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userDetails.Mobile ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var role in (userRoles ?? new List<SfpUserRoleMap>()))
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role.RoleName ?? ""));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? ""));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }
    }
}
