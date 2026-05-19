using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos
{
    public class ReactionSummaryDto
    {
        public int Like { get; set; }

        public int Love { get; set; }

        public int Laugh { get; set; }

        public int Wow { get; set; }

        public int Sad { get; set; }
    }
}
