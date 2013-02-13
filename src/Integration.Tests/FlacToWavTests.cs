using System.IO;
using FileTranscoder;
using FlacDecode;
using NUnit.Framework;

namespace Integration.Tests
{
 [TestFixture]
    public class FlacToWavTests
    {
        [Test]
        public void can_transcode_to_wav ()
        {
			string @in = @".\res\dtmf.flac";
            string @out = @".\res\dtmf_out.wav";
			
			File.Delete(@out);

            Decode.FlacToWav(@in, @out);

            Assert.That(File.Exists(@out));
        }
    }
}
