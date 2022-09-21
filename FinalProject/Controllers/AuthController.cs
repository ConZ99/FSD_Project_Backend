using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace FinalProject.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IUserService _userService;
        public string JwtToken = string.Empty;

        public AuthController(IConfiguration configuration, DataContext context, IUserService userService)
        {
            _configuration = configuration;
            _context = context;
            _userService = userService;
        }

        [HttpGet("GetMe"), Authorize]
        public ActionResult<object> GetMe()
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = _userService.GetMyName();
            var userRole = User?.FindFirstValue(ClaimTypes.Role);
            return Ok(new { userId, userName, userRole });
        }

        [HttpPost("register")]
        public async Task<ActionResult<Account>> Register(RegisterDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            if (!await VerifyEmail(request.Email))
                return BadRequest("User already exists.");

            var account = new Account();
            account.FirstName = request.FirstName;
            account.LastName = request.LastName;
            account.Email = request.Email;
            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;
            account.DateOfBirth = request.DateOfBirth;
            account.Phone = request.Phone;
            account.Address = request.Address;
            account.Funds = 1000;
            account.Admin = request.Admin;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(account);
        }

        private async Task<bool> VerifyEmail(string email)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Email == email);

            return account == null;
        }
        //ASIGURATE CA POTI EXTRAGE TOKENUL IN RESTUL CONTROALLELOR
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(x=> x.Email == request.Email);
            
            if (account == null)
            {
                return BadRequest("User does not exist.");
            }

            if(account.Email != request.Email)
            {
                return BadRequest("User not found.");
            }
            
            if(!VerifyPasswordHash(request.Password, account.PasswordHash, account.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            JwtToken = CreateToken(account);
            //var configFile = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //var settings = configFile.AppSettings.Settings;
            //var key = "AppSettings:SavedToken";
            //if (settings[key] == null)
            //{
            //    settings.Add(key, JwtToken);
            //}
            //else
            //{
            //    settings[key].Value = JwtToken;
            //}
            //configFile.Save(ConfigurationSaveMode.Modified);
            //System.Configuration.ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            return Ok(JwtToken);
        }

        private string CreateToken(Account account)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Admin == 1 ? "Admin" : "User")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
