using System;
using System.ComponentModel;
using System.IO;

namespace LocalStorage.Paging.Views
{
	/// <summary>
	/// A page which is responsible for maintaining the indices of all free (e.g. unused) pages.
	/// Indices with a value less than 1 are unused slots.
	/// </summary>
	internal sealed class FreePagesView
		: IPageView
	{
		public const int HeaderSize = 2*sizeof (int);

		private readonly FreePageList _freePages;
		private readonly Page _page;
		private readonly BinaryReader _reader;
		private readonly BinaryWriter _writer;

		public FreePagesView(Page page)
		{
			if (page == null) throw new ArgumentNullException("page");
			if (page.Descriptor.Type != PageType.FreePageIndex)
				throw new InvalidEnumArgumentException("page", (int)page.Descriptor.Type, typeof(PageType));

			_page = page;

			_reader = new BinaryReader(page);
			_writer = new BinaryWriter(page);
			_freePages = new FreePageList(_reader, _writer);
		}

		/// <summary>
		/// The index of the previous <see cref="FreePagesView"/> or 0 if this is the first.
		/// </summary>
		public int PreviousPageIndex
		{
			get
			{
				_reader.BaseStream.Position = 0;
				return _reader.ReadInt32();
			}
			set
			{
				_writer.BaseStream.Position = 0;
				_writer.Write(value);
			}
		}

		/// <summary>
		/// The index of the next <see cref="FreePagesView"/> or 0 if this is the last.
		/// </summary>
		public int NextPageIndex
		{
			get
			{
				_reader.BaseStream.Position = sizeof(int);
				return _reader.ReadInt32();
			}
			set
			{
				_writer.BaseStream.Position = sizeof(int);
				_writer.Write(value);
			}
		}

		public FreePageList FreePages
		{
			get { return _freePages; }
		}

		public void Dispose()
		{
			_page.Dispose();
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