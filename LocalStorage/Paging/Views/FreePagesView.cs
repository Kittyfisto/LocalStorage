using System;
using System.IO;

namespace LocalStorage.Paging.Views
{
	/// <summary>
	/// A page which is responsible for maintaining the indices of all free (e.g. unused) pages.
	/// Indices with a value less than 1 are unused slots.
	/// </summary>
	internal sealed class FreePagesView
		: AbstractLinkedListPageView
	{
		private readonly FreePageList _freePages;

		public FreePagesView(Page page)
			: base(page)
		{
			if (page.Descriptor.Type != PageType.FreePageIndex)
				throw new ArgumentException("page");

			_freePages = new FreePageList(Reader, Writer);
		}

		public FreePageList FreePages
		{
			get { return _freePages; }
		}

		public class FreePageList
		{
			private readonly BinaryReader _reader;
			private readonly BinaryWriter _writer;
			private readonly int _count;

			public FreePageList(BinaryReader reader, BinaryWriter writer)
			{
				_reader = reader;
				_writer = writer;
				_count = (int) ((_reader.BaseStream.Length - HeaderSize)/sizeof (int));
			}

			public int Count
			{
				get { return _count; }
			}

			public int this[int index]
			{
				get
				{
					_reader.BaseStream.Position = index*sizeof (int) + HeaderSize;
					return _reader.ReadInt32();
				}
				set
				{
					_writer.BaseStream.Position = index*sizeof (int) + HeaderSize;
					_writer.Write(value);
				}
			}
		}
	}
}