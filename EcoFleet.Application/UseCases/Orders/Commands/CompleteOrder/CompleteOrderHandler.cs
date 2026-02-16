using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Commands.CompleteOrder
{
    public class CompleteOrderHandler : IRequestHandler<CompleteOrderCommand>
    {
        private readonly IRepositoryOrder _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CompleteOrderHandler(IRepositoryOrder repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
        {
            var orderId = new OrderId(request.Id);
            var order = await _repository.GetByIdAsync(orderId, cancellationToken);

            if (order is null)
                throw new NotFoundException(nameof(Order), request.Id);

            order.Complete();

            await _repository.Update(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
