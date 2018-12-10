using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Network.API.Data;
using Network.API.Dtos;
using Network.API.Models;

namespace Network.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Policy = "AdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await (from user in _context.Users orderby user.UserName
                                  select new
                                  {
                                      user.Id,
                                      user.UserName,
                                      Roles = (from userRole in user.UserRoles
                                               join role in _context.Roles
                                               on userRole.RoleId
                                               equals role.Id
                                               select role.Name).ToList()
                                  }).ToListAsync();

            return Ok(userList);
        }

        [Authorize(Policy = "AdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var dbUser = await _userManager.FindByNameAsync(userName);
            var userRoles = await _userManager.GetRolesAsync(dbUser);

            var selectedRoles = roleEditDto.RoleNames ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(dbUser, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
                return BadRequest("Failed to add user to roles.");

            result = await _userManager.RemoveFromRolesAsync(dbUser, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
                return BadRequest("Failed to remove user from roles.");

            return Ok(await _userManager.GetRolesAsync(dbUser));

        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("moderatePhotos")]
        public async Task<IActionResult> ModeratePhotos()
        {
            return Ok();
        }
    }
}