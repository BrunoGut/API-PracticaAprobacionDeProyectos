using Application.Interfaces.IQuery;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Query
{
    public class ProjectProposalQuery : IProjectProposalQuery
    {
        private readonly AppDbContext _context;

        public ProjectProposalQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectProposal>> GetAllWithDetailsAsync()
        {
            return await _context.ProjectProposals
                .Include(p => p.ProjectArea)
                .Include(p => p.ProjectType)
                .Include(p => p.ApprovalStatus)
                .Include(p => p.ProjectApprovalSteps)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<ProjectProposal> GetProjectById(Guid id)
        {
            return await _context.ProjectProposals
                .Include(p => p.ProjectArea)
                .Include(p => p.ProjectType)
                .Include(p => p.ApprovalStatus)
                .Include(p => p.User).ThenInclude(u => u.ApproverRole)
                .Include(p => p.ProjectApprovalSteps)
                    .ThenInclude(s => s.User).ThenInclude(u => u.ApproverRole)
                .Include(p => p.ProjectApprovalSteps)
                    .ThenInclude(s => s.ApproverRole)
                .Include(p => p.ProjectApprovalSteps)
                    .ThenInclude(s => s.ApprovalStatus)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProjectProposal?> GetByTitleAsync(string title)
        {
            return await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Title.ToLower() == title.ToLower());
        }

    }
}
