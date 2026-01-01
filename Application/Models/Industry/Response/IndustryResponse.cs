using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Industry.Response
{
    public class IndustryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
