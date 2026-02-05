using EcoFleet.Domain.Common;
using EcoFleet.Domain.Exceptions;

namespace EcoFleet.Domain.ValueObjects
{
    public class LicensePlate : ValueObject
    {
        public string Value { get; }

        private LicensePlate(string value) => Value = value;

        public static LicensePlate Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("License plate cannot be empty.");

            return new LicensePlate(value.ToUpperInvariant());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
