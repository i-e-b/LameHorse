namespace WavReader
{
	public interface IReadPCM
	{
        int Read(short[] leftSamples, short[] rightSamples);
	}
}
