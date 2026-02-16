using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Commands.StartOrder
{
    public class StartOrderHandler : IRequestHandler<StartOrderCommand>
    {
        private readonly IRepositoryOrder _repository;
        private readonly IUnitOfWork _unitOfWork;

        public StartOrderHandler(IRepositoryOrder repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(StartOrderCommand request, CancellationToken cancellationToken)
        {
            var orderId = new OrderId(request.Id);
            var order = await _repository.GetByIdAsync(orderId, cancellationToken);

            if (order is null)
                throw new NotFoundException(nameof(Order), request.Id);

            order.StartProgress();

            await _repository.Update(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
