using EcoFleet.Application.UseCases.Orders.Commands.CancelOrder;
using EcoFleet.Application.UseCases.Orders.Commands.CompleteOrder;
using EcoFleet.Application.UseCases.Orders.Commands.CreateOrder;
using EcoFleet.Application.UseCases.Orders.Commands.StartOrder;
using EcoFleet.Application.UseCases.Orders.Queries.DTOs;
using EcoFleet.Application.UseCases.Orders.Queries.GetAllOrders;
using EcoFleet.Application.UseCases.Orders.Queries.GetOrderById;
using EcoFleet.Application.Utilities.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.API.Controllers
{
    /// <summary>
    /// Manages delivery orders — creation, lifecycle transitions, and queries.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Retrieves a paginated list of orders with optional filters.
        /// </summary>
        /// <param name="query">Optional filters: driver ID, status, and pagination parameters.</param>
        /// <returns>A paginated list of orders matching the specified filters.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedDTO<OrderDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOrders([FromQuery] GetAllOrdersQuery query)
        {
            var result = await _sender.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single order by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order.</param>
        /// <returns>The order details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var query = new GetOrderByIdQuery(id);
            var order = await _sender.Send(query);

            return Ok(order);
        }

        /// <summary>
        /// Creates a new delivery order assigned to a driver.
        /// </summary>
        /// <param name="command">The order creation data including driver ID, pick-up/drop-off coordinates, and price.</param>
        /// <returns>The unique identifier of the newly created order.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var orderId = await _sender.Send(command);

            return CreatedAtAction(nameof(CreateOrder), new { id = orderId }, orderId);
        }

        /// <summary>
        /// Starts a pending order, transitioning it to in-progress status.
        /// </summary>
        /// <param name="id">The unique identifier of the order to start.</param>
        [HttpPatch("start/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> StartOrder(Guid id)
        {
            var command = new StartOrderCommand(id);
            await _sender.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Completes an in-progress order, setting the finished date.
        /// </summary>
        /// <param name="id">The unique identifier of the order to complete.</param>
        [HttpPatch("complete/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CompleteOrder(Guid id)
        {
            var command = new CompleteOrderCommand(id);
            await _sender.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Cancels a pending or in-progress order. Cannot cancel a completed order.
        /// </summary>
        /// <param name="id">The unique identifier of the order to cancel.</param>
        [HttpPatch("cancel/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var command = new CancelOrderCommand(id);
            await _sender.Send(command);

            return NoContent();
        }
    }
}
