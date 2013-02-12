using System;
using System.IO;

namespace WavReader
{
	public class WavFromFile: IReadPCM, IPCMAudio
	{
		readonly FileStream _file;
		readonly BitSplitter _splitter;

		static readonly byte[] RiffHeader = new byte[] { 0x52, 0x49, 0x46, 0x46 };
		static readonly byte[] FormatWave = new byte[] { 0x57, 0x41, 0x56, 0x45 };
		static readonly byte[] FormatTag = new byte[] { 0x66, 0x6d, 0x74, 0x20 };
		static readonly byte[] AudioFormat = new byte[] { 0x01, 0x00 };
		static readonly byte[] SubchunkId = new byte[] { 0x64, 0x61, 0x74, 0x61 };

		public WavFromFile(FileStream file)
		{
			_file = file;
			_splitter = new BitSplitter(file);
			ReadHeaders();
		}

		void ReadHeaders()
		{
			if (! _splitter.CurrentIs(RiffHeader)) throw new Exception("Bad RIFF header");

			var len1 = _splitter.GetIntegerIntel(32);
			if (! _splitter.CurrentIs(FormatWave)) throw new Exception("Missing WAVE format");
			if (! _splitter.CurrentIs(FormatTag)) throw new Exception("Format tag missing");

			var metaDataSize = (int) _splitter.GetIntegerIntel(32);

			if (! _splitter.CurrentIs(AudioFormat)) throw new Exception("Audio type header missing");
			Channels = (int) _splitter.GetIntegerIntel(16);
			SampleRateHz = (int) _splitter.GetIntegerIntel(32);

			_splitter.SkipBytes(6);

			BitDepth = (int) _splitter.GetIntegerIntel(16);

			_splitter.SkipBytes(metaDataSize - 16);

			if (!_splitter.CurrentIs(SubchunkId)) throw new Exception("Sub chunk id wrong");

			var len2 = _splitter.GetIntegerIntel(32);

			if ((len1 - 42) != len2) throw new Exception("Header length or file length wrong");
		}

		public int Read(short[] leftSamples, short[] rightSamples)
		{
            var chanBits = BitDepth * Channels;
            var fileBits = (_file.Length - _file.Position) * 8;
            var fileSamples = fileBits / chanBits;
            var bufLen = Math.Min(leftSamples.Length, rightSamples.Length);
            var max = Math.Min(fileSamples, bufLen);

			for (int i = 0; i < max; i++)
			{
				unchecked // TODO: need a signed integer method!
				{
					leftSamples[i] = (short)_splitter.GetIntegerIntel(BitDepth);
					rightSamples[i] = (short)_splitter.GetIntegerIntel(BitDepth);
				}
			}

			return (int)max;
		}

		public int Channels { get; private set; }
		public int SampleRateHz { get; private set; }
		public int BitDepth { get; private set; }
	}
}