using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.API.Data;
using Network.API.Dtos;
using Network.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Network.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> GetUsers()
        {
            var dbUsers = await _repository.GetUsers();

            var users = _mapper.Map<IEnumerable<UserListDto>>(dbUsers);

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var dbUser = await _repository.GetUser(id);
            var user = _mapper.Map<UserDetailsDto>(dbUser);

            return Ok(user);
        }
    }
}