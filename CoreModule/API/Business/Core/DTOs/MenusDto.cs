using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Core.DTOs
{
    public class MenusDto
    {
        public string name { get; set; } = string.Empty;
        public string? icon { get; set; }
        public int moduleID { get; set; }
        public int parentMenuId { get; set; }
        public string? menuUrl { get; set; }
        public bool isActive { get; set; }
    }
}
