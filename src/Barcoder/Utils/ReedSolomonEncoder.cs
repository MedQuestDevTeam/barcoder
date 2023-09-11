using System;
using System.Collections.Generic;

namespace Barcoder.Utils
{
    internal class ReedSolomonEncoder
    {
        private readonly object _syncRoot = new object();
        private readonly List<GfPoly> _polynomes = new List<GfPoly>();

        public ReedSolomonEncoder(GaloisField galoisField)
        {
            GaloisField = galoisField;
            _polynomes = new List<GfPoly> { new GfPoly(GaloisField, new[] { 1 }) };
        }

        public GaloisField GaloisField { get; }

        public GfPoly[] Polynomes => _polynomes.ToArray();

        public int[] Encode(int[] data, int eccCount)
        {
            GfPoly generator = GetPolynomial(eccCount);
            GfPoly info = new GfPoly(GaloisField, data);
            info = info.MultiplyByMonominal(eccCount, 1);
            (_, GfPoly remainder) = info.Divide(generator);
            int[] result = new int[eccCount];
            int numZero = eccCount - remainder.Coefficients.Length;
            Array.Copy(remainder.Coefficients, 0, result, numZero, remainder.Coefficients.Length);
            return result;
        }

        private GfPoly GetPolynomial(int degree)
        {
            lock (_syncRoot)
            {
                if (degree >= _polynomes.Count)
                {
                    GfPoly last = _polynomes[_polynomes.Count - 1];
                    for (int d = _polynomes.Count; d <= degree; d++)
                    {
                        GfPoly next = last.Multiply(new GfPoly(GaloisField, new int[] { 1, GaloisField.ALogTable[d - 1 + GaloisField.Base] }));
                        _polynomes.Add(next);
                        last = next;
                    }
                }
                return _polynomes[degree];
            }
        }
    }
}
