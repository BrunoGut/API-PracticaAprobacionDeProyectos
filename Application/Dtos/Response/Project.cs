using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Response
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Duration { get; set; }
        public Users User { get; set; }
        public GenericResponse Area { get; set; }
        public GenericResponse Status { get; set; }
        public GenericResponse Type { get; set; }

        public List<ApprovalStep> Steps { get; set; }
    }
}
