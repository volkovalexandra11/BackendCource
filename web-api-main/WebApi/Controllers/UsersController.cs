using System;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{

    
    
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private IUserRepository userRepository;
        // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
        public UsersController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet("{userId}")]
        [Produces("application/json", "application/xml")]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var user = userRepository.FindById(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                FullName = $"{user.LastName} {user.FirstName}",
                Id = userId
            };
            return Ok(userDto);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserInfo user)
        {
            var createdUserEntity = new UserEntity()
            {
                Login = user.Login,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            
            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId = createdUserEntity.Id },
                createdUserEntity.Id);
        }
        
        public class UserInfo
        {
            public string Login { get; }
            public string FirstName { get; }
            public string LastName { get; }
     
            public UserInfo(string login, string firstName, string lastName)
            {
                Login = login;
                FirstName = firstName;
                LastName = lastName;
            }
        }
    }
}