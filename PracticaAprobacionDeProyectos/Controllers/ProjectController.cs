using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PracticaAprobacionDeProyectos.Controllers
{
    [Route("api")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IPorjectProposalService _proposalService;
        private readonly IProjectApprovalStepService _approvalStepService;

        public ProjectController(IPorjectProposalService proposalService, IProjectApprovalStepService approvalStepService)
        {
            _proposalService = proposalService;
            _approvalStepService = approvalStepService;
        }

        [HttpGet("Project")]
        [ProducesResponseType(typeof(List<ProjectShort>), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> GetProjects([FromQuery] string? title, [FromQuery] int? status, [FromQuery] int? applicant, [FromQuery] int? approvalUser)
        {
            try
            {
                var result = await _proposalService.GetFilteredProjectsAsync(title, status, applicant, approvalUser);
                return Ok(result);
            }
            catch (InvalidFilterParameterException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        [HttpPost("Project")]
        [ProducesResponseType(typeof(Project), 201)]
        [ProducesResponseType(typeof(ApiError), 400)]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreate request)
        {
            try
            {
                var result = await _proposalService.CreateProjectAsync(request);
                return StatusCode(201, result);
            }
            catch (InvalidProjectDataException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        [HttpPatch("Project/{id}/decision")]
        [ProducesResponseType(typeof(Project), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 409)]
        public async Task<IActionResult> PatchDecision(Guid id, [FromBody] DecisionStep request)
        {
            try
            {
                var updated = await _approvalStepService.ProcessStepDecisionAsync(id, request);
                return Ok(updated);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
            catch (InvalidDecisionDataException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        [HttpPatch("Project/{id}")]
        [ProducesResponseType(typeof(Project), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 409)]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdate request)
        {
            try
            {
                var result = await _proposalService.UpdateProposalAsync(id, request);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
            catch (InvalidDecisionDataException ex)
            {
                return BadRequest(new ApiError { Message = ex.Message });
            }
        }

        [HttpGet("Project/{id}")]
        [ProducesResponseType(typeof(Project), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _proposalService.GetProposalDetailByIdAsync(id);
                return Ok(result);
            }
            catch (InvalidDecisionDataException ex)
            {
                return BadRequest(new ApiError { Message=ex.Message});
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message});
            }
        }
    }
}
