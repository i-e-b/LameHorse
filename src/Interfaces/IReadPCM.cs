namespace Interfaces
{
	public interface IReadPCM
	{
        int Read(short[] leftSamples, short[] rightSamples);
	}
}
