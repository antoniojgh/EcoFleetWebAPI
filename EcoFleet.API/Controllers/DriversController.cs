using EcoFleet.Application.UseCases.Drivers.Commands.CreateDriver;
using EcoFleet.Application.UseCases.Drivers.Commands.DeleteDriver;
using EcoFleet.Application.UseCases.Drivers.Commands.ReinstateDriver;
using EcoFleet.Application.UseCases.Drivers.Commands.SuspendDriver;
using EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver;
using EcoFleet.Application.UseCases.Drivers.Queries.GetAllDrivers;
using EcoFleet.Application.UseCases.Drivers.Queries.GetDriverById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly ISender _sender;

        public DriversController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrivers([FromQuery] GetAllDriversQuery query)
        {
            var result = await _sender.Send(query);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(Guid id)
        {
            var query = new GetDriverByIdQuery(id);
            var driver = await _sender.Send(query);

            return Ok(driver);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverCommand command)
        {
            var driverId = await _sender.Send(command);

            return CreatedAtAction(nameof(CreateDriver), new { id = driverId }, driverId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] UpdateDriverCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the body.");
            }

            await _sender.Send(command);

            return NoContent();
        }

        [HttpPatch("suspend/{id}")]
        public async Task<IActionResult> SuspendDriver(Guid id)
        {
            var command = new SuspendDriverCommand(id);
            await _sender.Send(command);

            return NoContent();
        }

        [HttpPatch("reinstate/{id}")]
        public async Task<IActionResult> ReinstateDriver(Guid id)
        {
            var command = new ReinstateDriverCommand(id);
            await _sender.Send(command);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(Guid id)
        {
            var command = new DeleteDriverCommand(id);
            await _sender.Send(command);

            return NoContent();
        }
    }
}
