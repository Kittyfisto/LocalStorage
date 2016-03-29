using System;

namespace LocalStorage.Paging
{
	internal struct PageDescriptor
	{
		public readonly uint Id;
		public readonly long Offset;
		public readonly int Size;

		public static readonly int HeaderSize = sizeof (uint) + sizeof (int);

		public PageDescriptor(uint id, long offset, int size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException("size");

			Id = id;
			Offset = offset;
			Size = size;
		}
	}
}