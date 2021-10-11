using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class UserCreateDto
    {
        [Required]
        public string Login { get; set; }
            
        [DefaultValue("John")]
        public string FirstName { get; set; }
            
        [DefaultValue("Doe")]
        public string LastName { get; set; }
    }
    
    public class UserDto
    { 
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public int GamesPlayed { get; set; }
        public Guid? CurrentGameId { get; set; }
    }
}