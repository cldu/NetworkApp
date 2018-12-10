using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Network.API.Data;
using Network.API.Dtos;
using Network.API.Models;

namespace Network.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IConfiguration configuration, IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _configuration = configuration;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            var userToCreate = _mapper.Map<User>(userDto);

            var result = await _userManager.CreateAsync(userToCreate, userDto.Password);
            
            var userToReturn = _mapper.Map<UserDetailsDto>(userToCreate);

            if (result.Succeeded)
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var userToVerify = await _userManager.FindByNameAsync(userDto.Username);

            var result = await _signInManager.CheckPasswordSignInAsync(userToVerify, userDto.Password, false);

            if (result.Succeeded)
            {
                var dbUser = _userManager.Users.Include(p => p.Photos).SingleOrDefault(u => u.NormalizedUserName == userDto.Username.ToUpper());

                var user = _mapper.Map<UserListDto>(dbUser);

                return Ok(new
                {
                    token = GenerateJwtToken(dbUser).Result,
                    user
                });
            }

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User dbUser)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, dbUser.Id.ToString()),
                new Claim(ClaimTypes.Name, dbUser.UserName)
            };
            
            var roles = await _userManager.GetRolesAsync(dbUser);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}