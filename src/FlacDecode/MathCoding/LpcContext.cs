namespace FlacDecode.MathCoding
{
    /// <summary>
    /// Context for LPC coefficients calculation and order estimation
    /// </summary>
    unsafe public class LpcContext
    {
        public LpcContext()
        {
            coefs = new int[LinearPredictiveCoding.MaxLpcOrder];
            reflection_coeffs = new double[LinearPredictiveCoding.MaxLpcOrder];
            prediction_error = new double[LinearPredictiveCoding.MaxLpcOrder];
            autocorr_values = new double[LinearPredictiveCoding.MaxLpcOrder + 1];
            best_orders = new int[LinearPredictiveCoding.MaxLpcOrder];
            done_lpcs = new uint[LinearPredictiveCoding.MaxLpcPrecisions];
        }

        /// <summary>
        /// Reset to initial (blank) state
        /// </summary>
        public void Reset()
        {
            autocorr_order = 0;
            for (int iPrecision = 0; iPrecision < LinearPredictiveCoding.MaxLpcPrecisions; iPrecision++)
                done_lpcs[iPrecision] = 0;
        }

        /// <summary>
        /// Calculate autocorrelation data and reflection coefficients.
        /// Can be used to incrementaly compute coefficients for higher orders,
        /// because it caches them.
        /// </summary>
        /// <param name="order">Maximum order</param>
        /// <param name="samples">Samples pointer</param>
        /// <param name="blocksize">Block size</param>
        /// <param name="window">Window function</param>
        public void GetReflection(int order, int* samples, int blocksize, float* window)
        {
            if (autocorr_order > order)
                return;
            fixed (double* reff = reflection_coeffs, autoc = autocorr_values, err = prediction_error)
            {
                LinearPredictiveCoding.compute_autocorr(samples, blocksize, autocorr_order, order, autoc, window);
                LinearPredictiveCoding.compute_schur_reflection(autoc, (uint)order, reff, err);
                autocorr_order = order + 1;
            }
        }

        public void GetReflection1(int order, int* samples, int blocksize, float* window)
        {
            if (autocorr_order > order)
                return;
            fixed (double* reff = reflection_coeffs, autoc = autocorr_values, err = prediction_error)
            {
                LinearPredictiveCoding.compute_autocorr(samples, blocksize, 0, order + 1, autoc, window);
                for (int i = 1; i <= order; i++)
                    autoc[i] = autoc[i + 1];
                LinearPredictiveCoding.compute_schur_reflection(autoc, (uint)order, reff, err);
                autocorr_order = order + 1;
            }
        }

        public void ComputeReflection(int order, float* autocorr)
        {
            fixed (double* reff = reflection_coeffs, autoc = autocorr_values, err = prediction_error)
            {
                for (int i = 0; i <= order; i++)
                    autoc[i] = autocorr[i];
                LinearPredictiveCoding.compute_schur_reflection(autoc, (uint)order, reff, err);
                autocorr_order = order + 1;
            }
        }

        public void ComputeReflection(int order, double* autocorr)
        {
            fixed (double* reff = reflection_coeffs, autoc = autocorr_values, err = prediction_error)
            {
                for (int i = 0; i <= order; i++)
                    autoc[i] = autocorr[i];
                LinearPredictiveCoding.compute_schur_reflection(autoc, (uint)order, reff, err);
                autocorr_order = order + 1;
            }
        }

        public double Akaike(int blocksize, int order, double alpha, double beta)
        {
            //return (blocksize - order) * (Math.Log(prediction_error[order - 1]) - Math.Log(1.0)) + Math.Log(blocksize) * order * (alpha + beta * order);
            return blocksize * System.Math.Log(prediction_error[order - 1]) + System.Math.Log(blocksize) * order * (alpha + beta * order);
        }

        /// <summary>
        /// Sorts orders based on Akaike's criteria
        /// </summary>
        public void SortOrdersAkaike(int blocksize, int count, int min_order, int max_order, double alpha, double beta)
        {
            for (int i = min_order; i <= max_order; i++)
                best_orders[i - min_order] = i;
            int lim = max_order - min_order + 1;
            for (int i = 0; i < lim && i < count; i++)
            {
                for (int j = i + 1; j < lim; j++)
                {
                    if (Akaike(blocksize, best_orders[j], alpha, beta) < Akaike(blocksize, best_orders[i], alpha, beta))
                    {
                        int tmp = best_orders[j];
                        best_orders[j] = best_orders[i];
                        best_orders[i] = tmp;
                    }
                }
            }
        }

        /// <summary>
        /// Produces LPC coefficients from autocorrelation data.
        /// </summary>
        /// <param name="lpcs">LPC coefficients buffer (for all orders)</param>
        public void ComputeLPC(float* lpcs)
        {
            fixed (double* reff = reflection_coeffs)
                LinearPredictiveCoding.compute_lpc_coefs((uint)autocorr_order - 1, reff, lpcs);
        }

        public double[] autocorr_values;
	    readonly double[] reflection_coeffs;
        public double[] prediction_error;
        public int[] best_orders;
        public int[] coefs;
        int autocorr_order;
        public int shift;

        public double[] Reflection
        {
            get
            {
                return reflection_coeffs;
            }
        }

        public uint[] done_lpcs;
    }
}
