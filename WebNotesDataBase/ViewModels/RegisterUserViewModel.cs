﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebNotesDataBase.Models;

namespace WebNotesDataBase.ViewModels
{
    public class RegisterUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 4)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 4)]
        public string LastName { get; set; }

        public string Birthday { get; set; }

        [StringLength(4000, MinimumLength = 0)]
        public string About { get; set; }

        [Required]
        [StringLength(40, MinimumLength = 6)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(1000, MinimumLength = 8)]
        public string Pass { get; set; }
    }
}