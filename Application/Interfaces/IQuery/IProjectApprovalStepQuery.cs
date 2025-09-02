using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IProjectApprovalStepQuery
    {
        Task<List<ProjectApprovalStep>> GetStepsByProposalAsync(Guid proposalId);
        Task<ProjectApprovalStep> GetByIdAsync(long stepId);
    }
}
