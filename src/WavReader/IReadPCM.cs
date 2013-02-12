namespace WavReader
{
	public interface IReadPCM
	{
        int Read(int[] leftSamples, int[] rightSamples);
	}
}
