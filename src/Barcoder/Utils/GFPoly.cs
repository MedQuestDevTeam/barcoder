using System;
using System.Linq;

namespace Barcoder.Utils
{
    internal struct GfPoly
    {
        public GaloisField GaloisField;
        public int[] Coefficients;
        
        public GfPoly(GaloisField galoisField, int[] coefficients)
        {
            GaloisField = galoisField;
            Coefficients = coefficients.Length > 1 ? coefficients.SkipWhile(x => x == 0).ToArray() : coefficients;
        }

        public static GfPoly Zero(GaloisField galoisField)
            => new GfPoly(galoisField, new int[] { 0 });

        public static GfPoly MonominalPoly(GaloisField galoisField, int degree, int coefficient)
        {
            if (coefficient == 0) return Zero(galoisField);
            int[] coefficients = new int[degree + 1];
            coefficients[0] = coefficient;
            return new GfPoly(galoisField, coefficients);
        }

        public int Degree() => Coefficients.Length - 1;

        public bool IsZero() => Coefficients[0] == 0;

        public int GetCoefficient(int degree) => Coefficients[Degree() - degree];

        public GfPoly AddOrSubtract(GfPoly other)
        {
            if (IsZero()) return other;
            if (other.IsZero()) return this;
            int[] smallCoefficients = Coefficients;
            int[] largeCoefficients = other.Coefficients;
            if (smallCoefficients.Length > largeCoefficients.Length)
                (largeCoefficients, smallCoefficients) = (smallCoefficients, largeCoefficients);
            int[] sumDiff = new int[largeCoefficients.Length];
            int lenDiff = largeCoefficients.Length - smallCoefficients.Length;
            Array.Copy(largeCoefficients, sumDiff, lenDiff);
            for (int i = lenDiff; i < largeCoefficients.Length; i++)
                sumDiff[i] = GaloisField.AddOrSubtract(smallCoefficients[i - lenDiff], largeCoefficients[i]);
            return new GfPoly(GaloisField, sumDiff);
        }

        public GfPoly MultiplyByMonominal(int degree, int coefficient)
        {
            if (coefficient == 0) return Zero(GaloisField);
            int size = Coefficients.Length;
            int[] coefficients = new int[size + degree];
            for (int i = 0; i < size; i++)
                coefficients[i] = GaloisField.Multiply(Coefficients[i], coefficient);
            return new GfPoly(GaloisField, coefficients);
        }

        public GfPoly Multiply(GfPoly other)
        {
            if (IsZero() || other.IsZero()) return Zero(GaloisField);
            int[] product = new int[Coefficients.Length + other.Coefficients.Length - 1];
            for (int i = 0; i < Coefficients.Length; i++)
            {
                int ac = Coefficients[i];
                for (int j = 0; j < other.Coefficients.Length; j++)
                {
                    int bc = other.Coefficients[j];
                    product[i + j] = GaloisField.AddOrSubtract(product[i + j], GaloisField.Multiply(ac, bc));
                }
            }
            return new GfPoly(GaloisField, product);
        }

        public (GfPoly Quotient, GfPoly Remainder) Divide(GfPoly other)
        {
            GfPoly quotient = Zero(GaloisField);
            GfPoly remainder = this;
            int denomLeadTerm = other.GetCoefficient(other.Degree());
            int inversDenomLeadTerm = GaloisField.Inverse(denomLeadTerm);
            while (remainder.Degree() >= other.Degree() && !remainder.IsZero())
            {
                int degreeDiff = remainder.Degree() - other.Degree();
                int scale = GaloisField.Multiply(remainder.GetCoefficient(remainder.Degree()), inversDenomLeadTerm);
                GfPoly term = other.MultiplyByMonominal(degreeDiff, scale);
                GfPoly itQuot = MonominalPoly(GaloisField, degreeDiff, scale);
                quotient = quotient.AddOrSubtract(itQuot);
                remainder = remainder.AddOrSubtract(term);
            }
            return (quotient, remainder);
        }
    }
}
