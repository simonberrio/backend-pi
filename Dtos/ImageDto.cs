using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos
{
    public class ImageDto
    {
        public int EventId { get; set; }
        public IFormFile FormFile { get; set; }
    }
}
