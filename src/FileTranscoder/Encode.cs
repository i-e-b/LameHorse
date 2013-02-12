using System.IO;
using LameHorse;
using WavReader;

namespace FileTranscoder
{
	public class Encode
	{
        public static void WavToMp3(string sourceWavPath, string targetMp3Path, int bitRateKbps)
        {
            
            using (var @in = new FileStream(@".\res\dtmf.wav", FileMode.Open))
            using (var @out = new FileStream(@".\res\dtmf_out.mp3", FileMode.Create))
            {
                var reader = new WavFromFile(@in);
                var writer = new Mp3Writer(@out, bitRateKbps, reader);

                var left = new short[512];
                var right = new short[512];

                int len;
                while ((len = reader.Read(left,right)) > 0)
                {
                    writer.Write(left, right, len);
                }
                writer.Flush();
            }
        }
	}
}
