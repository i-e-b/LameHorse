using System;
using System.IO;
using LameHorse;

namespace LameExample
{
	public class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("LAME version "+ LibMp3Lame.GetLameVersion());

            using (var file = new FileStream(@"C:\temp\sample.mp3", FileMode.Create))
            {

	            using (var lame = new LibMp3Lame())
	            {
		            lame.LameInit();
		            lame.LameSetErrorFunction(ErrorFunc);
		            lame.LameSetBRate(256);
		            lame.LameSetMode(LibMp3Lame.MPEG_mode.JOINT_STEREO);
		            lame.LameSetInSampleRate(44100);
		            lame.LameSetNumChannels(2);
		            lame.LameSetQuality(9);
		            lame.LameInitParams();

		            var buffer = new byte[88000];
					var amp = short.MaxValue * 0.85d;
					var left = new short[22050];
					var right = new short[22050];

					for (int j = 0; j < 22050; j++)
					{
						left[j] = (short)(Math.Sin(j / 30.0) * amp);
						right[j] = (short)(Math.Cos(j / 40.0) * amp);
					}
		            for (int i = 0; i < 10; i++)
		            {
                        var size = lame.LameEncodeBuffer(left, right, 22050, buffer);
                        if (size < 1) throw new Exception("Empty frame");

                        file.Write(buffer, 0, size);
		            }

					var tail = lame.LameEncodeFlush(buffer);
					file.Write(buffer, 0, tail);
                    file.Flush();
				}
            }

			Console.WriteLine("Done. Press [enter]");
			Console.ReadKey();
		}

		static void ErrorFunc(string format, object[] args)
		{
			Console.WriteLine("Error occurred: "+ format); // won't work properly, but will print something.
		}
	}
}
