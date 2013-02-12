using System;
using System.IO;
using System.Threading;
using WavReader;

namespace LameHorse
{
	public class Mp3Writer : IWritePCM, IDisposable
	{
		readonly FileStream _file;
		LibMp3Lame _lame;
		readonly byte[] buffer = new byte[88000];

		public Mp3Writer(FileStream file, int bitRateKbps, IPCMAudio spec)
		{
			_file = file;
			_lame = new LibMp3Lame();
			_lame.LameInit();

			_lame.LameSetBRate(bitRateKbps);
			_lame.LameSetInSampleRate(spec.SampleRateHz);

            switch (spec.Channels)
            {
                case 1:
		            _lame.LameSetNumChannels(1);
		            _lame.LameSetMode(LibMp3Lame.MPEG_mode.MONO);
                    break;
				case 2:
		            _lame.LameSetNumChannels(2);
		            _lame.LameSetMode(LibMp3Lame.MPEG_mode.JOINT_STEREO);
                    break;
				default:
                    throw new Exception("Channel count must be 1 or 2");
            }
			_lame.LameSetQuality(9);
			_lame.LameInitParams();
		}

		public void Write(short[] leftSamples, short[] rightSamples, int length)
		{
			var size = _lame.LameEncodeBuffer(leftSamples, rightSamples, length, buffer);
			_file.Write(buffer, 0, size);
		}

		public void Flush()
		{
			int tail;
			while ((tail = _lame.LameEncodeFlush(buffer)) > 0)
			{
				_file.Write(buffer, 0, tail);
			}
			_file.Flush();
		}

		public void Dispose()
		{
			var tmp = Interlocked.Exchange(ref _lame, null);
			if (tmp == null) return;
			tmp.Dispose();
		}

		~Mp3Writer()
		{
			Dispose();
		}
	}
}