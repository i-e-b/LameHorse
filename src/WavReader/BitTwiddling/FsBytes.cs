using System.IO;

namespace WavReader
{
	public class FsBytes : IByteArray
	{
		readonly FileStream _fsData;

		public FsBytes(FileStream fsData)
		{
			_fsData = fsData;
		}

		public byte this[long index]
		{
			get
			{
                if (index != _fsData.Position) _fsData.Seek(index, SeekOrigin.Begin);
				return (byte)_fsData.ReadByte();
			}
		}
	}
}