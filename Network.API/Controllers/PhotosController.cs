using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Network.API.Data;
using Network.API.Dtos;
using Network.API.Helpers;
using Network.API.Models;

namespace Network.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly INetworkRepository _repository;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public PhotosController(INetworkRepository repository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repository = repository;
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;

            Account account = new Account(_cloudinaryConfig.Value.CloudName, _cloudinaryConfig.Value.ApiKey, _cloudinaryConfig.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var dbPhoto = await _repository.GetPhoto(id);

            var photo = _mapper.Map<PhotoDetailsDto>(dbPhoto);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoCreationDto photoCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var dbUser = await _repository.GetUser(userId);

            var file = photoCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(600).Height(600).Crop("fill").Gravity("auto")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoCreationDto.Url = uploadResult.Uri.ToString();
            photoCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoCreationDto);

            if(!dbUser.Photos.Any(p => p.IsProfilePhoto))
                photo.IsProfilePhoto = true;

            dbUser.Photos.Add(photo);

            if (await _repository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoDetailsDto>(photo);
                
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }
            
            return BadRequest("Error with adding the photo.");
        }

        [HttpPost("{id}/setProfile")]
        public async Task<IActionResult> SetProfilePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var dbUser = await _repository.GetUser(userId);

            if (!dbUser.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var dbPhoto = await _repository.GetPhoto(id);

            if (dbPhoto.IsProfilePhoto)
                return BadRequest("This photo is already the profile picture");

            var currentProfilePhoto = await _repository.GetUserProfilePhoto(userId);

            currentProfilePhoto.IsProfilePhoto = false;

            dbPhoto.IsProfilePhoto = true;

            if (await _repository.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo as the profile picture");

        }

        [HttpDelete("{photoId}")]
        public async Task<IActionResult> DeletePhoto(int userId, int photoId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var dbUser = await _repository.GetUser(userId);

            if (!dbUser.Photos.Any(p => p.Id == photoId))
                return Unauthorized();

            var dbPhoto = await _repository.GetPhoto(photoId);

            if (dbPhoto.IsProfilePhoto)
                return BadRequest("Profile photo can't be deleted.");

            if(dbPhoto.PublicId != null)
            {
                var deleteParams = new DeletionParams(dbPhoto.PublicId);

                var deleteResult = _cloudinary.Destroy(deleteParams);

                if (deleteResult.Result == "ok")
                    _repository.Delete(dbPhoto);
            }
            else
            {
                _repository.Delete(dbPhoto);
            }
            
            if (await _repository.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}