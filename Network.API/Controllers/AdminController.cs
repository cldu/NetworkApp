using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Network.API.Data;
using Network.API.Dtos;
using Network.API.Helpers;
using Network.API.Models;

namespace Network.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly Cloudinary _cloudinary;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;

        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _context = context;
            _userManager = userManager;
            _cloudinaryConfig = cloudinaryConfig;

            var acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
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
            var photos = await _context.Photos.Include(p => p.User)
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(u => new
                {
                    u.Id,
                    u.User.UserName,
                    u.Url,
                    u.IsApproved
                }).ToListAsync();

            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.Id == photoId);

            photo.IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.Id == photoId);
            
            if(photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);

                var deleteResult = _cloudinary.Destroy(deleteParams);

                if (deleteResult.Result == "ok")
                    _context.Photos.Remove(photo);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}