using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class DecisionStep
    {
        public long Id { get; set; }
        public int User { get; set; }
        public int Status { get; set; }
        public string Observation { get; set; }
    }
}
