using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class ProjectUpdate
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
    }
}
