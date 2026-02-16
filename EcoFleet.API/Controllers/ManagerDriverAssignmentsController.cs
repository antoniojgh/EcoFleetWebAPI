using EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.ActivateAssignment;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.CreateAssignment;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.DeactivateAssignment;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.DTOs;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAllAssignments;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAssignmentById;
using EcoFleet.Application.Utilities.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.API.Controllers
{
    /// <summary>
    /// Manages manager-driver assignments — creation, deactivation, and queries.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ManagerDriverAssignmentsController : ControllerBase
    {
        private readonly ISender _sender;

        public ManagerDriverAssignmentsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Retrieves a paginated list of manager-driver assignments with optional filters.
        /// </summary>
        /// <param name="query">Optional filters: manager ID, driver ID, active status, and pagination parameters.</param>
        /// <returns>A paginated list of assignments matching the specified filters.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedDTO<ManagerDriverAssignmentDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAssignments([FromQuery] GetAllAssignmentsQuery query)
        {
            var result = await _sender.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single manager-driver assignment by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the assignment.</param>
        /// <returns>The assignment details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ManagerDriverAssignmentDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssignmentById(Guid id)
        {
            var query = new GetAssignmentByIdQuery(id);
            var assignment = await _sender.Send(query);

            return Ok(assignment);
        }

        /// <summary>
        /// Creates a new manager-driver assignment.
        /// </summary>
        /// <param name="command">The assignment data including manager ID and driver ID.</param>
        /// <returns>The unique identifier of the newly created assignment.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentCommand command)
        {
            var assignmentId = await _sender.Send(command);

            return CreatedAtAction(nameof(CreateAssignment), new { id = assignmentId }, assignmentId);
        }

        /// <summary>
        /// Deactivates an active manager-driver assignment.
        /// </summary>
        /// <param name="id">The unique identifier of the assignment to deactivate.</param>
        [HttpPatch("deactivate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeactivateAssignment(Guid id)
        {
            var command = new DeactivateAssignmentCommand(id);
            await _sender.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Activates an inactive manager-driver assignment.
        /// </summary>
        /// <param name="id">The unique identifier of the assignment to activate.</param>
        [HttpPatch("activate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ActivateAssignment(Guid id)
        {
            var command = new ActivateAssignmentCommand(id);
            await _sender.Send(command);

            return NoContent();
        }
    }
}
