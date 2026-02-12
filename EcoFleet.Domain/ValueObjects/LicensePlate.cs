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

        /// <summary>
        /// Attempts to create a new instance of the LicensePlate class from the specified string representation.
        /// </summary>
        /// <remarks>If the input value is null or consists only of white-space characters, the method
        /// returns null. The input value is converted to uppercase using the invariant culture before creating the
        /// LicensePlate instance.</remarks>
        /// <param name="value">The string representation of the license plate to parse. This value must not be null or consist only of
        /// white-space characters.</param>
        /// <returns>A LicensePlate instance if the input value is valid; otherwise, null.</returns>
        public static LicensePlate? TryCreate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return new LicensePlate(value.ToUpperInvariant());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
