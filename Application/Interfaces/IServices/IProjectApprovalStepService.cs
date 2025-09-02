using Application.Dtos.Request;
using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface IProjectApprovalStepService
    {
        Task<Project> ProcessStepDecisionAsync(Guid proposalId, DecisionStep request);  
    }
}
