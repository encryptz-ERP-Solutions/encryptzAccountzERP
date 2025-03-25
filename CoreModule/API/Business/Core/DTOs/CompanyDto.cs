using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Core.DTOs
{
    public class CompanyDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string TanNo { get; set; }
        public string PanNo { get; set; }
        public int? BusinessTypeId { get; set; }
        public string Address { get; set; }
        public int? NationId { get; set; }
        public int? StateId { get; set; }
        public int? DistrictId { get; set; }
        public string Pin { get; set; }
        public string Gstin { get; set; }
        public string Epf { get; set; }
        public string Esi { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneNo { get; set; }
    }
}
