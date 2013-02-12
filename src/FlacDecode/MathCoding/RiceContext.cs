using FlacDecode.Flac;

namespace FlacDecode.MathCoding
{
	public class RiceContext
    {
        public RiceContext()
        {
            RiceParams = new int[Flake.MaxPartitions];
            BpsIfUsingEscape = new int[Flake.MaxPartitions];
        }
        /// <summary>
        /// partition order
        /// </summary>
        public int PartitionOrder;

        /// <summary>
        /// coding method: rice parameters use 4 bits for coding_method 0 and 5 bits for coding_method 1
        /// </summary>
        public int CodingMethod;

        /// <summary>
        /// Rice parameters
        /// </summary>
        public int[] RiceParams;

        /// <summary>
        /// bps if using escape code
        /// </summary>
        public int[] BpsIfUsingEscape;
    };
}
