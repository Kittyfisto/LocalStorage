using System;
using System.Text;

namespace LocalStorage.Paging.Views
{
	internal sealed class StringListView
		: AbstractLinkedListPageView
	{
		private readonly int _length;
		private const int StringDataOffset = sizeof (int);
		private ushort _freeOffset;

		public StringListView(Page page)
			: base(page)
		{
			if (page.Descriptor.Type != PageType.StringList)
				throw new ArgumentException("page");

			_length = (int) (page.Length - StringDataOffset);
			_freeOffset = StringDataOffset;
		}

		public bool TryAdd(int index, string value)
		{
			if (index <= 0)
				throw new ArgumentOutOfRangeException("index", "An index must be greater than 0");

			var requiredLength = _freeOffset + sizeof (int)*2 + value.Length * sizeof(char);
			if (requiredLength > _length)
				return false;

			Writer.BaseStream.Position = _freeOffset;
			Writer.Write(index);
			var data = Encoding.Unicode.GetBytes(value);
			Writer.Write(data.Length);
			Writer.Write(data);
			_freeOffset = (ushort)Writer.BaseStream.Position;
			return true;
		}

		public bool TryFind(int index, out string value)
		{
			Reader.BaseStream.Position = StringDataOffset;
			while (Reader.BaseStream.Position < _length)
			{
				int actualIndex = Reader.ReadInt32();
				if (actualIndex == 0) //< end of page...
					break;

				int stringLength = Reader.ReadInt32();
				if (actualIndex == index)
				{
					var data = Reader.ReadBytes(stringLength);
					value = Encoding.Unicode.GetString(data);
					return true;
				}

				Reader.BaseStream.Position += stringLength;
			}

			value = null;
			return false;
		}
	}
}