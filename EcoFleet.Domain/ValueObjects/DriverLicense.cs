using EcoFleet.Domain.Common;
using EcoFleet.Domain.Exceptions;

namespace EcoFleet.Domain.ValueObjects
{
    public class DriverLicense : ValueObject
    {
        public string Value { get; }

        private DriverLicense(string value) => Value = value;

        public static DriverLicense Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Driver license cannot be empty.");

            if (value.Trim().Length > 20)
                throw new DomainException("Driver license is too long.");

            return new DriverLicense(value.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
