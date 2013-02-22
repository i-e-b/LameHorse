using System;
using System.IO;
using System.Threading;

namespace FlacDecode
{
    public class WavWriter : IDisposable
    {
		FileStream _fs;

	    public WavWriter(string filePath, uint expectedBytes, int channels, int samplesSec)
	    {
		    _fs = new FileStream(filePath, FileMode.CreateNew);
            WriteHeader(_fs, expectedBytes, channels, samplesSec);
	    }

        public void WriteSamples(byte[] data, int offset, int count)
        {
            _fs.Write(data, offset, count);
        }
		

	    public void WriteSample(short sample)
	    {
			_fs.WriteByte((byte)(sample&0x00FF));
			_fs.WriteByte((byte)((sample&0xFF00)>>8));
	    }

		static readonly byte[] RiffHeader = new byte[] { 0x52, 0x49, 0x46, 0x46 };
		static readonly byte[] FormatWave = new byte[] { 0x57, 0x41, 0x56, 0x45 };
		static readonly byte[] FormatTag = new byte[] { 0x66, 0x6d, 0x74, 0x20 };
		static readonly byte[] AudioFormat = new byte[] { 0x01, 0x00 };
		static readonly byte[] SubchunkId = new byte[] { 0x64, 0x61, 0x74, 0x61 };
		private const int BytesPerSample = 2;

		public static void WriteHeader(Stream file, uint expectedBytes, int channels, int samplesSec)
		{
			var byteRate = (uint)(samplesSec * channels * BytesPerSample);
			var blockAlign = (uint)(channels * BytesPerSample);

			file.Write(RiffHeader, 0, RiffHeader.Length);
			file.Write(PackInt(expectedBytes + 42, 4), 0, 4);

			file.Write(FormatWave, 0, FormatWave.Length);
			file.Write(FormatTag, 0, FormatTag.Length);
			file.Write(PackInt(16, 4), 0, 4);

			file.Write(AudioFormat, 0, AudioFormat.Length);
			file.Write(PackInt((uint)channels), 0, 2);
			file.Write(PackInt((uint)samplesSec, 4), 0, 4);
			file.Write(PackInt(byteRate, 4), 0, 4);
			file.Write(PackInt(blockAlign), 0, 2);
			file.Write(PackInt(BytesPerSample * 8), 0, 2);
			file.Write(SubchunkId, 0, SubchunkId.Length);
			file.Write(PackInt(expectedBytes, 4), 0, 4);
		}

		static byte[] PackInt(uint source, int length = 2)
		{
			var retVal = new byte[length];
			retVal[0] = (byte)(source & 0xFF);
			retVal[1] = (byte)((source >> 8) & 0xFF);
			if (length == 4)
			{
				retVal[2] = (byte)((source >> 0x10) & 0xFF);
				retVal[3] = (byte)((source >> 0x18) & 0xFF);
			}
			return retVal;
		}

		~WavWriter()
		{
            Dispose();
		}

	    public void Dispose()
	    {
            var lfs = Interlocked.Exchange(ref _fs, null);
            if (lfs == null) return;
            lfs.Flush(true);
            lfs.Close();
            lfs.Dispose();
	    }

	    public void FlushAndClose()
	    {
            Dispose();
	    }
    }
}
