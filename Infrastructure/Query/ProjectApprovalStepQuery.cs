using Application.Interfaces.IQuery;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Query
{
    public class ProjectApprovalStepQuery : IProjectApprovalStepQuery
    {
        private readonly AppDbContext _context;

        public ProjectApprovalStepQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectApprovalStep>> GetStepsByProposalAsync(Guid proposalId)
        {
            return await _context.ProjectApprovalSteps
                .Where(s => s.ProjectProposalId == proposalId)
                .ToListAsync();
        }

        public async Task<ProjectApprovalStep> GetByIdAsync(long stepId)
        {
            return await _context.ProjectApprovalSteps
                .FirstOrDefaultAsync(s => s.Id == stepId);
        }
    }
}
