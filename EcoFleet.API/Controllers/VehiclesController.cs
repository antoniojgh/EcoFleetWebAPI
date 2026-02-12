using EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle;
using EcoFleet.Application.UseCases.Vehicles.Commands.DeleteVehicle;
using EcoFleet.Application.UseCases.Vehicles.Commands.MarkForMaintenance;
using EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle;
using EcoFleet.Application.UseCases.Vehicles.Queries.DTOs;
using EcoFleet.Application.UseCases.Vehicles.Queries.GetAllVehicle;
using EcoFleet.Application.UseCases.Vehicles.Queries.GetVehicleById;
using EcoFleet.Application.Utilities.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.API.Controllers
{
    /// <summary>
    /// Manages the vehicle fleet — CRUD operations, driver assignments, and maintenance workflows.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class VehiclesController : ControllerBase
    {
        private readonly ISender _sender;

        public VehiclesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Retrieves a paginated list of vehicles with optional filters.
        /// </summary>
        /// <param name="query">Optional filters: plate, status, location, driver, and pagination parameters.</param>
        /// <returns>A paginated list of vehicles matching the specified filters.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedDTO<VehicleDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllVehicles([FromQuery] GetAllVehicleQuery query)
        {
            var result = await _sender.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single vehicle by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle.</param>
        /// <returns>The vehicle details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VehicleDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVehicleById(Guid id)
        {
            var query = new GetVehicleByIdQuery(id);
            var vehicle = await _sender.Send(query);

            return Ok(vehicle);
        }

        /// <summary>
        /// Creates a new vehicle and optionally assigns a driver.
        /// </summary>
        /// <param name="command">The vehicle creation data including license plate, location, and optional driver ID.</param>
        /// <returns>The unique identifier of the newly created vehicle.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleCommand command)
        {
            var vehicleId = await _sender.Send(command);

            return CreatedAtAction(nameof(CreateVehicle), new { id = vehicleId }, vehicleId);
        }

        /// <summary>
        /// Updates an existing vehicle's plate, location, and driver assignment.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle to update.</param>
        /// <param name="command">The updated vehicle data.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the body.");
            }

            await _sender.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Marks an idle vehicle for maintenance. Fails if the vehicle is currently active.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle to mark for maintenance.</param>
        [HttpPatch("maintenance/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> MarkForMaintenance(Guid id)
        {
            var command = new MarkForMaintenanceCommand(id);
            await _sender.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Permanently deletes a vehicle from the fleet.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle to delete.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var command = new DeleteVehicleCommand(id);
            await _sender.Send(command);

            return NoContent();
        }
    }
}
