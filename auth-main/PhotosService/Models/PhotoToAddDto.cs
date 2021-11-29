﻿using System.ComponentModel.DataAnnotations;

namespace PhotosService.Models
{
    public class PhotoToAddDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [Required]
        public string Base64Content { get; set; }
    }
}
