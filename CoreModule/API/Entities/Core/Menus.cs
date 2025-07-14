using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Core
{
    public class Menus
    {

        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? icon { get; set; }
        public int moduleID { get; set; }
        public int parentMenuId { get; set; }
        public string? menuUrl { get; set; }
        public bool isActive { get; set; }

    }
}
