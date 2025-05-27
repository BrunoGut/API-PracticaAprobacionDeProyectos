using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Exceptions;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase
{
    public class ProjectApprovalStepService : IProjectApprovalStepService
    {
        private readonly IProjectApprovalStepCommand _stepCommand;
        private readonly IProjectProposalCommand _proposalCommand;
        private readonly IProjectApprovalStepQuery _stepQuery;
        private readonly IProjectProposalQuery _proposalQuery;
        private readonly IUserQuery _userQuery;

        public ProjectApprovalStepService(IProjectApprovalStepCommand stepCommand, IProjectProposalCommand proposalCommand, IProjectApprovalStepQuery stepQuery, IProjectProposalQuery proposalQuery, IUserQuery userQuery)
        {
            _stepCommand = stepCommand;
            _proposalCommand = proposalCommand;
            _stepQuery = stepQuery;
            _proposalQuery = proposalQuery;
            _userQuery = userQuery;
        }

        public async Task<StepDecisionResponse> ProcessStepDecisionAsync(Guid proposalId, StepDecisionRequest request)
        {
            var proposal = await _proposalQuery.GetProjectById(proposalId);
            if (proposal == null)
                throw new NotFoundException("Proyecto no encontrado");

            if (proposal.Status != 1 && proposal.Status != 4)
                throw new ConflictException("El proyecto ya no se encuentra en un estado que permite modificaciones");

            var stepId = request.GetStepId();
            var currentStep = await _stepQuery.GetByIdAsync(stepId);
            if (currentStep == null || (currentStep.Status != 1 && currentStep.Status != 4) || currentStep.ProjectProposalId != proposalId)
                throw new InvalidDecisionDataException("Datos de decisión inválidos");

            var allSteps = await _stepQuery.GetStepsByProposalAsync(proposalId);
            if (allSteps == null || !allSteps.Any())
                throw new InvalidDecisionDataException("Datos de decisión inválidos.");

            var previousStep = allSteps
                .Where(s => s.StepOrder < currentStep.StepOrder && s.Status != 3)
                .OrderByDescending(s => s.StepOrder)
                .FirstOrDefault();

            if (previousStep != null && previousStep.Status != 2)
                throw new InvalidDecisionDataException("Datos de decisión inválidos.");

            var groupedSteps = allSteps
                .Where(s => s.StepOrder == currentStep.StepOrder &&
                            s.ApproverRoleId == currentStep.ApproverRoleId &&
                            (s.Status == 1 || s.Status == 4))
                .ToList();

            int newStatus = request.Status;
            if (newStatus != 2 && newStatus != 3 && newStatus != 4)
                throw new InvalidDecisionDataException("Datos de decisión inválidos.");

            var user = await _userQuery.GetByIdAsync(request.User);
            if (user == null)
                throw new InvalidDecisionDataException("Usuario inválido.");

            bool isValidApprover = groupedSteps.Any(s => s.ApproverUserId == request.User);
            if (!isValidApprover)
            {
                isValidApprover = user.Role == currentStep.ApproverRoleId && groupedSteps.All(s => s.ApproverUserId == null);
            }

            if (!isValidApprover)
                throw new InvalidDecisionDataException("Datos de decisión inválidos.");

            foreach (var step in groupedSteps)
            {
                step.Status = newStatus;
                step.Observations = request.Observation;
                step.DecisionDate = DateTime.UtcNow;
                step.ApproverUserId = request.User;
                await _stepCommand.UpdateStepAsync(step);
            }

            if (newStatus == 3)
            {
                proposal.Status = 3;
                await _proposalCommand.UpdateProposalStatusAsync(proposal);
            }
            else if (newStatus == 4)
            {
                proposal.Status = 4;
                await _proposalCommand.UpdateProposalStatusAsync(proposal);
            }
            else
            {
                var remaining = allSteps.Where(s => s.Status == 1 || s.Status == 4).ToList();

                if (remaining.Any())
                {
                    if (proposal.Status != 1)
                    {
                        proposal.Status = 1;
                        await _proposalCommand.UpdateProposalStatusAsync(proposal);
                    }
                }
                else
                {
                    if (proposal.Status != 2)
                    {
                        proposal.Status = 2;
                        await _proposalCommand.UpdateProposalStatusAsync(proposal);
                    }
                }
            }


            proposal = await _proposalQuery.GetProjectById(proposalId);
            return new StepDecisionResponse
            {
                Id = proposal.Id,
                Title = proposal.Title,
                Description = proposal.Description,
                Amount = proposal.EstimatedAmount,
                Duration = proposal.EstimatedDuration,
                Area = new GenericResponse
                {
                    Id = proposal.Area,
                    Name = proposal.ProjectArea?.Name ?? "Área desconocida"
                },
                Status = new GenericResponse
                {
                    Id = proposal.Status,
                    Name = proposal.ApprovalStatus?.Name ?? "Estado desconocido"
                },
                Type = new GenericResponse
                {
                    Id = proposal.Type,
                    Name = proposal.ProjectType?.Name ?? "Tipo desconocido"
                },
                User = new UserResponse
                {
                    Id = proposal.CreateBy,
                    Name = proposal.User?.Name ?? "Desconocido",
                    Email = proposal.User?.Email ?? "N/A",
                    Role = proposal.User?.ApproverRole != null
                        ? new GenericResponse
                        {
                            Id = proposal.User.Role,
                            Name = proposal.User.ApproverRole.Name
                        }
                        : new GenericResponse()
                },
                Steps = proposal.ProjectApprovalSteps.Select(s => new StepResponse
                {
                    Id = s.Id.ToString(),
                    StepOrder = s.StepOrder,
                    DecisionDate = s.DecisionDate,
                    Observations = s.Observations,
                    ApproverUser = s.User != null
        ? new UserResponse
        {
            Id = s.User.Id,
            Name = s.User.Name,
            Email = s.User.Email,
            Role = new GenericResponse
            {
                Id = s.User.Role,
                Name = s.User.ApproverRole?.Name ?? "Desconocido"
            }
        }
        : new UserResponse(),
                    ApproverRole = new GenericResponse
                    {
                        Id = s.ApproverRoleId,
                        Name = s.ApproverRole?.Name ?? "Desconocido"
                    },
                    Status = new GenericResponse
                    {
                        Id = s.Status,
                        Name = s.ApprovalStatus?.Name ?? "Desconocido"
                    }
                }).ToList(),
            };
        }
    }
}
