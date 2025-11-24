//using Google.Cloud.Firestore.Converters; // Required for FirestoreConverter<T>
//using Google.Protobuf;
//using System; // Required for Math.Round, ArgumentException, OverflowException
//using System.Globalization; // Required for CultureInfo.InvariantCulture, NumberStyles

//namespace AirplaneBooking.Infrastructure.FirestoreConverters;

///// <summary>
///// A custom Firestore converter for System.Decimal that stores values as long integers
///// (e.g., representing currency in cents) to preserve precision, and converts them back to decimal.
///// This converter is suitable for financial values that typically have a fixed number of decimal places.
///// </summary>
//public class DecimalConverter : FirestoreConverter<decimal> // <--- Class name is DecimalConverter
//{
//    // Multiplier to convert decimal to its smallest integer unit (e.g., 100 for cents/two decimal places)
//    private const decimal Multiplier = 100m;

//    /// <summary>
//    /// Converts a value from its Firestore representation (long) to a System.Decimal.
//    /// </summary>
//    /// <param name="context">The read context (unused in this converter).</param>
//    /// <param name="value">The value read from Firestore. Expected to be a long.</param>
//    /// <returns>The converted decimal value.</returns>
//    /// <exception cref="ArgumentException">Thrown if the value cannot be converted to a long.</exception>
//    public override decimal FromFirestore(ReadContext context, object value)
//    {
//        if (value is long l)
//        {
//            // Convert long (e.g., 123 cents) back to decimal (e.g., 1.23)
//            return l / Multiplier;
//        }
//        // Add robust handling for other potential storage types if previous data exists or schema is flexible.
//        // For example, if some decimals might have been stored as doubles directly in Firestore:
//        if (value is double db)
//        {
//            // IMPORTANT: Converting from double to decimal can lead to precision loss for exact financial values.
//            // Log a warning if this path is hit, as it indicates unexpected data format.
//            Console.WriteLine($"Warning: Reading double {db} from Firestore for decimal conversion in '{nameof(DecimalConverter)}'. Precision loss may occur.");
//            return (decimal)db;
//        }
//        // If some decimals were stored as strings (another common pattern for arbitrary precision):
//        if (value is string s && decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedStringDecimal))
//        {
//            // This path is for compatibility if you previously stored as string
//            Console.WriteLine($"Info: Reading string '{s}' from Firestore for decimal conversion in '{nameof(DecimalConverter)}'.");
//            return parsedStringDecimal;
//        }

//        throw new ArgumentException(
//            $"Cannot convert Firestore value of type '{value?.GetType().Name ?? "null"}' to System.Decimal in '{nameof(DecimalConverter)}'. " +
//            $"Expected a long integer representation. Value: {value}");
//    }

//    /// <summary>
//    /// Converts a System.Decimal value to its Firestore representation (long).
//    /// </summary>
//    /// <param name="context">The write context (unused in this converter).</param>
//    /// <param name="value">The decimal value to convert. Expected to have precision compatible with Multiplier.</param>
//    /// <returns>The converted long integer.</returns>
//    /// <exception cref="ArgumentException">Thrown if the decimal value is too large or causes overflow during conversion to long.</exception>
//    public override object ToFirestore(WriteContext context, decimal value)
//    {
//        // Convert decimal to long (e.g., $1.23 becomes 123)
//        // Math.Round is crucial to ensure correct rounding to the nearest cent.
//        // MidpointRounding.AwayFromZero is a common financial rounding rule.
//        long longValue;
//        try
//        {
//            longValue = (long)Math.Round(value * Multiplier, MidpointRounding.AwayFromZero);
//        }
//        catch (OverflowException ex)
//        {
//            // Log and re-throw if the value is too large for a long after multiplication
//            throw new ArgumentException(
//                $"Decimal value {value} is too large to be converted to long (after multiplying by {Multiplier}) " +
//                $"in '{nameof(DecimalConverter)}'.", ex);
//        }

//        return longValue;
//    }
//}