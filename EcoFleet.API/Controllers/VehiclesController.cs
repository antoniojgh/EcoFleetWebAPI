using EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle;
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

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleCommand command)
        {
            // We send the command to the Application Layer
            // If validation fails, the Middleware catches the exception automatically.
            var vehicleId = await _sender.Send(command);

            return CreatedAtAction(nameof(CreateVehicle), new { id = vehicleId }, vehicleId);
        }
    }
}
