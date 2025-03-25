﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Admin.DTOs
{
    public class UserDto
    {
        public string userId { get; set; }=string.Empty;
        public string userName { get; set; } = string.Empty;
        public string? userPassword { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? panNo { get; set; } = string.Empty;
        public string? adharCardNo { get; set; } = string.Empty;
        public string? phoneNo { get; set; } = string.Empty;
        public string? address { get; set; } = string.Empty;
        public int? stateId { get; set; } = 0;
        public int? nationId { get; set; } = 0;
        public bool isActive { get; set; } = true;

    }
}
