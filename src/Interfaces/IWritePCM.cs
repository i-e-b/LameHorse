namespace LameHorse
{
	public interface IWritePCM
	{
        int Write(int[] leftSamples, int[] rightSamples);
        void Flush();
	}
}
