using FluentValidation;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle
{
    public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleCommand>
    {
        public UpdateVehicleValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Vehicle Id is required.");

            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage("License plate is required.")
                .MaximumLength(15).WithMessage("License plate too long.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Invalid Latitude.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Invalid Longitude.");
        }
    }
}
