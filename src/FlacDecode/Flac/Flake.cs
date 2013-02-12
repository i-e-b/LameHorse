/**
 * CUETools.Flake: pure managed FLAC audio encoder
 * Copyright (c) 2009 Grigory Chudov
 * Based on Flake encoder, http://flake-enc.sourceforge.net/
 * Copyright (c) 2006-2009 Justin Ruggles
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

namespace FlacDecode.Flac
{
	public class Flake
	{
		public const int MaxBlocksize = 65535;
		public const int MaxRiceParam = 14;
		public const int MaxPartitionOrder = 8;
		public const int MaxPartitions = 1 << MaxPartitionOrder;

		public const int FlacStreamMetadataSeekpointSampleNumberLen = 64; /* bits */
		public const int FlacStreamMetadataSeekpointStreamOffsetLen = 64; /* bits */
		public const int FlacStreamMetadataSeekpointFrameSamplesLen = 16; /* bits */


		public const int flacStreamMetadataStreaminfoMinBlockSizeLen = 16; /* bits */
		public const int flacStreamMetadataStreaminfoMaxBlockSizeLen = 16; /* bits */
		public const int flacStreamMetadataStreaminfoMinFrameSizeLen = 24; /* bits */
		public const int flacStreamMetadataStreaminfoMaxFrameSizeLen = 24; /* bits */
		public const int flacStreamMetadataStreaminfoSampleRateLen = 20; /* bits */
		public const int flacStreamMetadataStreaminfoChannelsLen = 3; /* bits */
		public const int flacStreamMetadataStreaminfoBitsPerSampleLen = 5; /* bits */
		public const int flacStreamMetadataStreaminfoTotalSamplesLen = 36; /* bits */
		public const int flacStreamMetadataStreaminfoMd5SumLen = 128; /* bits */

		public static readonly int[] flac_samplerates = new[] {
				0, 0, 0, 0,
				8000, 16000, 22050, 24000, 32000, 44100, 48000, 96000,
				0, 0, 0, 0
			};
		public static readonly int[] flac_blocksizes = new[] { 0, 192, 576, 1152, 2304, 4608, 0, 0, 256, 512, 1024, 2048, 4096, 8192, 16384 };
		public static readonly int[] flac_bitdepths = new[] { 0, 8, 12, 0, 16, 20, 24, 0 };

	}
}
