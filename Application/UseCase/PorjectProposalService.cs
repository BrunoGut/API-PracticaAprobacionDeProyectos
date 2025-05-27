using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
//using Application.Helpers;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase
{
    public class PorjectProposalService : IPorjectProposalService
    {
        private readonly IProjectProposalCommand _proposalCommand;
        private readonly IProjectApprovalStepCommand _stepCommand;
        private readonly IUserQuery _userQuery;
        private readonly IApprovalRuleQuery _ruleQuery;
        private readonly IProjectProposalQuery _proposalQuery;
        private readonly IAreaQuery _areaQuery;
        private readonly IProjectTypeQuery _typeQuery;
        private readonly IApprovalStatusQuery _approvalStatusQuery;

        public PorjectProposalService(IProjectProposalCommand proposalCommand, IProjectApprovalStepCommand stepCommand, IUserQuery userQuery, IApprovalRuleQuery ruleQuery, IProjectProposalQuery proposalQuery, IAreaQuery areaQuery, IProjectTypeQuery typeQuery, IApprovalStatusQuery approvalStatusQuery)
        {
            _proposalCommand = proposalCommand;
            _stepCommand = stepCommand;
            _userQuery = userQuery;
            _ruleQuery = ruleQuery;
            _proposalQuery = proposalQuery;
            _areaQuery = areaQuery;
            _typeQuery = typeQuery;
            _approvalStatusQuery = approvalStatusQuery;
        }

        public async Task<StepDecisionResponse> CreateProjectAsync(ProjectProposalRequest request)
        {
            var existingTitle = await _proposalQuery.GetByTitleAsync(request.Title);
            if (existingTitle != null)
                throw new InvalidProjectDataException("El título del proyecto ya existe.");

            if (request.EstimatedDuration <= 0)
                throw new InvalidProjectDataException("La duración estimada debe ser mayor a cero.");


            var existingArea = await _areaQuery.GetByIdAsync(request.AreaId);
            if (existingArea == null)
                throw new InvalidProjectDataException("Área inválida.");

            var existingType = await _typeQuery.GetByIdAsync(request.TypeId);
            if (existingType == null)
                throw new InvalidProjectDataException("Tipo inválido.");

            var existingUser = await _userQuery.GetByIdAsync(request.User);
            if (existingUser == null)
                throw new InvalidProjectDataException("Usuario inválido.");

            var proposal = new ProjectProposal
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                EstimatedAmount = request.EstimatedAmount,
                EstimatedDuration = request.EstimatedDuration,
                Area = request.AreaId,
                Type = request.TypeId,
                CreateBy = request.User,
                CreateAt = DateTime.UtcNow,
                Status = 1
            };

            await _proposalCommand.CreateProject(proposal);

            var rules = await _ruleQuery.GetAllAsync();
            var users = await _userQuery.GetAllAsync();

            decimal amount = proposal.EstimatedAmount;
            int area = proposal.Area;
            int type = proposal.Type;

            bool InRange(decimal value, decimal min, decimal? max) =>
                value >= min && (max == 0 || value <= max);

            var steps = new List<ProjectApprovalStep>();

            var rulesGrouped = rules
                .Where(r => InRange(amount, r.MinAmount, r.MaxAmount))
                .GroupBy(r => r.StepOrder);

            foreach (var group in rulesGrouped)
            {
                var stepOrder = group.Key;

                var selectedRule =
                    group.FirstOrDefault(r => r.Area == area && r.Type == type) ??
                    group.FirstOrDefault(r => r.Area == area && r.Type == null) ??
                    group.FirstOrDefault(r => r.Type == type && r.Area == null) ??
                    group.FirstOrDefault(r => r.Area == null && r.Type == null);

                if (selectedRule == null) continue;

                var approvers = users
                    .Where(u => u.Role == selectedRule.ApproverRoleId)
                    .ToList();

                if (approvers.Count == 1)
                {
                    var singleUser = approvers.First();
                    steps.Add(new ProjectApprovalStep
                    {
                        Id = GenerateRandomBigIntId(),
                        ProjectProposalId = proposal.Id,
                        ApproverRoleId = selectedRule.ApproverRoleId,
                        ApproverUserId = singleUser.Id,
                        StepOrder = selectedRule.StepOrder,
                        Status = 1,
                        Observations = "Pendiente"
                    });
                }
                else if (approvers.Count > 1)
                {
                    steps.Add(new ProjectApprovalStep
                    {
                        Id = GenerateRandomBigIntId(),
                        ProjectProposalId = proposal.Id,
                        ApproverRoleId = selectedRule.ApproverRoleId,
                        ApproverUserId = null,
                        StepOrder = selectedRule.StepOrder,
                        Status = 1,
                        Observations = "Pendiente (sin usuario asignado)"
                    });
                }

                else
                {
                    steps.Add(new ProjectApprovalStep
                    {
                        Id = GenerateRandomBigIntId(),
                        ProjectProposalId = proposal.Id,
                        ApproverRoleId = selectedRule.ApproverRoleId,
                        ApproverUserId = null,
                        StepOrder = selectedRule.StepOrder,
                        Status = 1,
                        Observations = "Pendiente (sin usuario asignado)"
                    });
                }
            }

            await _stepCommand.SaveStepsAsync(steps);

            var fullProposal = await _proposalQuery.GetProjectById(proposal.Id);

            return new StepDecisionResponse
            {
                Id = fullProposal.Id,
                Title = fullProposal.Title,
                Description = fullProposal.Description,
                Amount = fullProposal.EstimatedAmount,
                Duration = fullProposal.EstimatedDuration,
                Area = new GenericResponse
                {
                    Id = fullProposal.Area,
                    Name = fullProposal.ProjectArea?.Name ?? "Área desconocida"
                },
                Status = new GenericResponse
                {
                    Id = fullProposal.Status,
                    Name = fullProposal.ApprovalStatus?.Name ?? "Desconocido"
                },
                Type = new GenericResponse
                {
                    Id = fullProposal.Type,
                    Name = fullProposal.ProjectType?.Name ?? "Tipo desconocido"
                },
                User = new UserResponse
                {
                    Id = fullProposal.CreateBy,
                    Name = fullProposal.User?.Name ?? "Desconocido",
                    Email = fullProposal.User?.Email ?? "N/A",
                    Role = fullProposal.User?.ApproverRole != null
            ? new GenericResponse
            {
                Id = fullProposal.User.Role,
                Name = fullProposal.User.ApproverRole.Name
            }
            : new GenericResponse()
                },
                Steps = fullProposal.ProjectApprovalSteps.Select(s => new StepResponse
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
                        : null,
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
                }).ToList()
            };
        }

        private static System.Numerics.BigInteger GenerateRandomBigIntId()
        {
            var random = new Random();
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);
            return new System.Numerics.BigInteger(bytes);
        }

        public async Task<List<GetProjectResponse>> GetFilteredProjectsAsync(string? title, int? status, int? applicant, int? approvalUser)
        {
            if (status.HasValue)
            {
                if (status.Value <= 0)
                    throw new InvalidFilterParameterException("Parámetro de consulta inválido");

                var statusExists = await _approvalStatusQuery.ExistsAsync(status.Value);
                if (!statusExists)
                    throw new InvalidFilterParameterException("Parámetro de consulta inválido");
            }

            if (applicant.HasValue)
            {
                if (applicant.Value <= 0)
                    throw new InvalidFilterParameterException("Parámetro de consulta inválido");

                var userExists = await _userQuery.ExistsAsync(applicant.Value);
                if (!userExists)
                    throw new InvalidFilterParameterException("Parámetro de consulta inválido");
            }

            if (approvalUser.HasValue)
            {
                if (approvalUser.Value <= 0)
                    throw new InvalidFilterParameterException("Parámetro de consulta inválido");

                var userExists = await _userQuery.ExistsAsync(approvalUser.Value);
                if (!userExists)
                    throw new InvalidFilterParameterException("Parámetro de consulta inválido");
            }

            var proposals = await _proposalQuery.GetAllWithDetailsAsync();

            if (!string.IsNullOrWhiteSpace(title))
                proposals = proposals.Where(p => p.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();

            if (status.HasValue)
                proposals = proposals.Where(p => p.Status == status.Value).ToList();

            if (applicant.HasValue)
                proposals = proposals.Where(p => p.CreateBy == applicant.Value).ToList();

            if (approvalUser.HasValue)
            {
                proposals = proposals
                    .Where(p => p.ProjectApprovalSteps.Any(s => s.ApproverUserId == approvalUser.Value))
                    .ToList();
            }

            return proposals.Select(p => new GetProjectResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Amount = p.EstimatedAmount,
                Duration = p.EstimatedDuration,
                Area = p.ProjectArea.Name,
                Status = p.ApprovalStatus.Name,
                Type = p.ProjectType.Name,
            }).ToList();
        }


        public async Task<StepDecisionResponse> UpdateProposalAsync(Guid id, UpdateProjectProposalRequest request)
        {
            var proposal = await _proposalQuery.GetProjectById(id);
            if (proposal == null)
                throw new NotFoundException("Proyecto no encontrado");

            if (proposal.Status != 4)
                throw new ConflictException("El proyecto ya no se encuentra en un estado que permite modificaciones");

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description) || request.Duration <= 0)
                throw new InvalidDecisionDataException("Datos de actualización inválidos");

            proposal.Title = request.Title;
            proposal.Description = request.Description;
            proposal.EstimatedDuration = request.Duration;

            await _proposalCommand.UpdateProposalStatusAsync(proposal);

            
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
                            Role = s.User.ApproverRole != null
                                ? new GenericResponse
                                {
                                    Id = s.User.Role,
                                    Name = s.User.ApproverRole.Name
                                }
                                : new GenericResponse()
                        }
                        : new UserResponse(),
                    ApproverRole = s.ApproverRole != null
                        ? new GenericResponse
                        {
                            Id = s.ApproverRoleId,
                            Name = s.ApproverRole.Name
                        }
                        : new GenericResponse(),
                    Status = s.ApprovalStatus != null
                        ? new GenericResponse
                        {
                            Id = s.Status,
                            Name = s.ApprovalStatus.Name
                        }
                        : new GenericResponse()
                }).ToList()
            };
        }

        public async Task<StepDecisionResponse> GetProposalDetailByIdAsync(Guid id)
        {
            var proposal = await _proposalQuery.GetProjectById(id);
            if (proposal == null)
                throw new NotFoundException("Proyecto no encontrado");

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
                            Role = s.User.ApproverRole != null
                                ? new GenericResponse
                                {
                                    Id = s.User.Role,
                                    Name = s.User.ApproverRole.Name
                                }
                                : new GenericResponse()
                        }
                        : new UserResponse(),
                    ApproverRole = s.ApproverRole != null
                        ? new GenericResponse
                        {
                            Id = s.ApproverRoleId,
                            Name = s.ApproverRole.Name
                        }
                        : new GenericResponse(),
                    Status = s.ApprovalStatus != null
                        ? new GenericResponse
                        {
                            Id = s.Status,
                            Name = s.ApprovalStatus.Name
                        }
                        : new GenericResponse()
                }).ToList()
            };
        }
    }
}

