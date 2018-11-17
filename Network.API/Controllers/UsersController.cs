using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.API.Data;
using Network.API.Dtos;
using Network.API.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Network.API.Helpers;

namespace Network.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivity))]
    public class UsersController : ControllerBase
    {
        private readonly INetworkRepository _repository;
        private readonly IMapper _mapper;

        public UsersController(INetworkRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentUser = await _repository.GetUser(currentUserId);
            userParams.UserId = currentUserId;
            
            var dbUsers = await _repository.GetUsers(userParams);

            var users = _mapper.Map<IEnumerable<UserListDto>>(dbUsers);

            Response.AddPagination(dbUsers.PageNumber, dbUsers.PageSize, dbUsers.TotalCount, dbUsers.TotalPages);

            return Ok(users);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var dbUser = await _repository.GetUser(id);
            var user = _mapper.Map<UserDetailsDto>(dbUser);

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var dbUser = await _repository.GetUser(id);

            _mapper.Map(userUpdateDto, dbUser);

            if (await _repository.SaveAll())
                return NoContent();

            throw new Exception($"Updating user with {id} failed on save.");
        }
    }
}