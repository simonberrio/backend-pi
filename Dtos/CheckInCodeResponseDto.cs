using System;

namespace Dtos
{
    public class CheckInCodeResponseDto
    {
        public int EventId { get; set; }
        public Guid CheckInCode { get; set; }
    }
}
