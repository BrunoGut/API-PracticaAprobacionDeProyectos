using Application.Dtos.Request;
using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface IPorjectProposalService
    {
        Task<StepDecisionResponse> CreateProjectAsync(ProjectProposalRequest request);
        Task<List<GetProjectResponse>> GetFilteredProjectsAsync(string? title, int? status, int? applicant, int? approvalUser);
        Task<StepDecisionResponse> UpdateProposalAsync(Guid id, UpdateProjectProposalRequest request);
        Task<StepDecisionResponse> GetProposalDetailByIdAsync(Guid id);
    }
}
