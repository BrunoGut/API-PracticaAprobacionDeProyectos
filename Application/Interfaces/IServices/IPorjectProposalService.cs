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
        Task<Project> CreateProjectAsync(ProjectCreate request);
        Task<List<ProjectShort>> GetFilteredProjectsAsync(string? title, int? status, int? applicant, int? approvalUser);
        Task<Project> UpdateProposalAsync(Guid id, ProjectUpdate request);
        Task<Project> GetProposalDetailByIdAsync(Guid id);
    }
}
