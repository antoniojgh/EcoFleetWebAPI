using FluentValidation;

namespace EcoFleet.Application.UseCases.Orders.Commands.CreateOrder
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty().WithMessage("Driver Id is required.");

            RuleFor(x => x.PickUpLatitude)
                .InclusiveBetween(-90, 90).WithMessage("Pick-up latitude must be between -90 and 90.");

            RuleFor(x => x.PickUpLongitude)
                .InclusiveBetween(-180, 180).WithMessage("Pick-up longitude must be between -180 and 180.");

            RuleFor(x => x.DropOffLatitude)
                .InclusiveBetween(-90, 90).WithMessage("Drop-off latitude must be between -90 and 90.");

            RuleFor(x => x.DropOffLongitude)
                .InclusiveBetween(-180, 180).WithMessage("Drop-off longitude must be between -180 and 180.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
        }
    }
}
