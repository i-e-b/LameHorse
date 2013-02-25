using System;
using FlacDecode;
using FlacDecode.LibFlac;

namespace FileTranscoder
{
	public static class Decode
	{

		public static void FlacToWav(string sourceFlac, string targetNewWav)
		{
			Using.LibFlac(sourceFlac, targetNewWav);
		}

		public static class Using
		{
			public static void Flake(string sourceFlac, string targetNewWav)
			{
				using (var reader = new FlakeReader(sourceFlac))
				{
					var expectedBytes = (reader.Length * reader.PCM.BitsPerSample * reader.PCM.ChannelCount) / 8;

					if (expectedBytes > uint.MaxValue) throw new Exception("Too many samples for a wav");

					var buf = new AudioBuffer(reader, 4096);

					using (var wav = new WavWriter(targetNewWav, (uint)expectedBytes, reader.PCM.ChannelCount, reader.PCM.SampleRate))
					{
						while (reader.Read(buf, 4096) > 0)
						{
							wav.WriteSamples(buf.Bytes, 0, buf.ByteLength);
						}
						wav.FlushAndClose();
					}
				}
			}

			public static void LibFlac(string sourceFlac, string targetNewWav)
			{
				var decoder = new LibFlacDecode(sourceFlac, targetNewWav);
				decoder.DecodeFlacToWav();
			}
		}
	}
}
