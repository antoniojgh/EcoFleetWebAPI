using EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle;
using EcoFleet.Application.UseCases.Vehicles.Commands.DeleteVehicle;
using EcoFleet.Application.UseCases.Vehicles.Commands.MarkForMaintenance;
using EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle;
using EcoFleet.Application.UseCases.Vehicles.Queries.GetAllVehicle;
using EcoFleet.Application.UseCases.Vehicles.Queries.GetVehicleById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly ISender _sender;

        public VehiclesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles([FromQuery] GetAllVehicleQuery query)
        {
            var result = await _sender.Send(query);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(Guid id)
        {
            var query = new GetVehicleByIdQuery(id);
            var vehicle = await _sender.Send(query);

            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleCommand command)
        {
            // We send the command to the Application Layer
            // If validation fails, the Middleware catches the exception automatically.
            var vehicleId = await _sender.Send(command);

            return CreatedAtAction(nameof(CreateVehicle), new { id = vehicleId }, vehicleId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the body.");
            }

            await _sender.Send(command);

            return NoContent();
        }

        [HttpPatch("maintenance/{id}")]
        public async Task<IActionResult> MarkForMaintenance(Guid id)
        {
            var command = new MarkForMaintenanceCommand(id);
            await _sender.Send(command);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var command = new DeleteVehicleCommand(id);
            await _sender.Send(command);

            return NoContent();
        }
    }
}
