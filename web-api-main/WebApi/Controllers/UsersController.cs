using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public UsersController(IUserRepository userRepository, 
            IMapper mapper, 
            LinkGenerator linkGenerator)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpHead("{userId}")]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        [Produces("application/json", "application/xml")]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var user = userRepository.FindById(userId);
            if (user == null)
                return NotFound();
            return Ok(mapper.Map<UserDto>(user));
        }

        [HttpPost]
        [Produces("application/json", "application/xml")]
        public IActionResult CreateUser([FromBody] UserCreationDto user)
        {
            if (user == null)
                return BadRequest();

            var loginIsValid = string.IsNullOrEmpty(user.Login) 
                               || !user.Login.All(char.IsLetterOrDigit);
            
            if (loginIsValid) 
                ModelState.AddModelError(nameof(UserCreationDto.Login), "default login");
            
            if (!ModelState.IsValid) 
                return UnprocessableEntity(ModelState);
            
            var userEntity = userRepository.Insert(mapper.Map<UserEntity>(user));
            return CreatedAtRoute(nameof(GetUserById),
                new {userId = userEntity.Id},
                userEntity.Id);
        }
        
        [HttpPut("{userId}", Name = nameof(UpdateUser))]
        [Produces("application/json", "application/xml")]
        public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UserUpdatingDto user)
        {
            if (user is null || userId.Equals(Guid.Empty))
                return BadRequest();
            
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            user.Id = userId;
            var userEntity = mapper.Map<UserEntity>(user);
            
            userRepository.UpdateOrInsert(userEntity, out var isInserted);

            return !isInserted
                ? NoContent()
                : CreatedAtRoute(nameof(UpdateUser),
                    new {userId = userEntity.Id},
                    userEntity.Id);
        }
        
        [HttpPatch("{userId}", Name = nameof(PartiallyUpdateUser))]
        [Produces("application/json", "application/xml")]
        public IActionResult PartiallyUpdateUser(
            [FromRoute] Guid userId, 
            [FromBody] JsonPatchDocument<UserUpdatingDto> patchDoc
            )
        {
            if (patchDoc is null)
                return BadRequest();

            var user = userRepository.FindById(userId);
            if (user is null)
                return NotFound();
            
            var updatingDto = mapper.Map<UserUpdatingDto>(user);
            patchDoc.ApplyTo(updatingDto, ModelState);

            if (!TryValidateModel(updatingDto))
                return UnprocessableEntity(ModelState);
            
            userRepository.Update(mapper.Map(updatingDto, user));
            return NoContent();
        }
        
        [HttpDelete("{userId:guid}", Name = nameof(DeleteUser))]
        public IActionResult DeleteUser([FromRoute] Guid userId)
        {
            if (userRepository.FindById(userId) == null)
                return NotFound();
            
            userRepository.Delete(userId);
            return NoContent();
        }

        [HttpOptions]
        public IActionResult Options()
        {
            Response.Headers.Add("Allow", new []{"GET", "POST", "OPTIONS"});
            return Ok();
        }
        
        [HttpGet(Name = nameof(GetUsers))]
        [Produces("application/json", "application/xml")]
        public IActionResult GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Min(20, Math.Max(1, pageSize));
            
            var pageList = userRepository.GetPage(pageNumber, pageSize);
            var users = mapper.Map<IEnumerable<UserDto>>(pageList);
            
            var previousPageLink = GetPreviousPageLink(pageNumber, pageSize);
            var paginationHeader = GetPaginationHeader(pageNumber, pageSize, previousPageLink);
            
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));
            return Ok(users);
        }

        private object GetPaginationHeader(int pageNumber, int pageSize, string previousPageLink)
        {
            return new
            {
                previousPageLink,
                nextPageLink = linkGenerator
                    .GetUriByRouteValues(HttpContext, 
                        "GetUsers", 
                        new {pageNumber = pageNumber + 1, pageSize}
                    ),
                totalCount = 4,
                pageSize,
                currentPage = pageNumber,
                totalPages = 4
            };
        }

        private string GetPreviousPageLink(int pageNumber, int pageSize)
        {
            if (pageNumber > 1)
                return linkGenerator
                    .GetUriByRouteValues(HttpContext,
                        "GetUsers",
                        new {pageNumber = pageNumber - 1, pageSize}
                    );
            return null;
        }
    }
}