using System.IO;
using FileTranscoder;
using FlacDecode;
using NUnit.Framework;

namespace Integration.Tests
{
    [TestFixture]
	public class RoundTripTest
	{
        [Test]
        public void can_round_trip_from_flac_to_mp3 ()
        {
			string @in = @".\res\dtmf.flac";
            string @mezz = @".\res\dtmf_out.wav";
            string @out = @".\res\dtmf_out.mp3";
			
			File.Delete(@mezz);
			File.Delete(@out);

            Decode.FlacToWav(@in, @mezz);

            Assert.That(File.Exists(@mezz));

            Encode.WavToMp3(@mezz, @out, 256);

            Assert.That(File.Exists(@out));
        }
	}
}
