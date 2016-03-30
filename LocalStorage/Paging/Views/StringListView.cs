using System;

namespace LocalStorage.Paging.Views
{
	internal sealed class StringListView
		: AbstractLinkedListPageView
	{
		private readonly int _length;
		private const int StringCountOffset = HeaderSize;
		private const int StringDataOffset = StringCountOffset + sizeof (int);
		private ushort _freeOffset;

		public StringListView(Page page)
			: base(page)
		{
			if (page.Descriptor.Type != PageType.StringList)
				throw new ArgumentException("page");

			_length = (int) (page.Length - StringDataOffset);
			_freeOffset = StringDataOffset;
		}

		public int StringCount
		{
			get
			{
				Reader.BaseStream.Position = StringCountOffset;
				return Reader.ReadInt32();
			}
			set
			{
				Writer.BaseStream.Position = StringCountOffset;
				Writer.Write(value);
			}
		}

		public void Add(int index, string value)
		{
			Writer.BaseStream.Position = _freeOffset;
			Writer.Write(index);
			Writer.Write(value);
			_freeOffset = (ushort)Writer.BaseStream.Position;
			++StringCount;
		}

		public string Find(int index)
		{
			Reader.BaseStream.Position = StringDataOffset;
			while (Reader.BaseStream.Position < _length)
			{
				int actualIndex = Reader.ReadInt32();

				// TODO: Write length into stream and then simply skip the given amount of bytes...
				var value = Reader.ReadString();
				if (actualIndex == index)
				{
					return value;
				}
			}

			throw new NotImplementedException();
		}
	}
}