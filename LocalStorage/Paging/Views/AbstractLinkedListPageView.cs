using System.IO;

namespace LocalStorage.Paging.Views
{
	/// <summary>
	///     Base class for page views that are part of a linked list.
	/// </summary>
	internal abstract class AbstractLinkedListPageView
		: AbstractPageView
	{
		/// <summary>
		///     The size of the header reserved by this view.
		///     The range of [0, HeaderSize] shall not be overwritten by subclasses.
		/// </summary>
		protected const int HeaderSize = sizeof (int)*2;

		protected const int PreviousPageIndexOffset = 0;
		protected const int NextPageIndexOffset = PreviousPageIndexOffset + sizeof (int);

		protected readonly BinaryReader Reader;
		protected readonly BinaryWriter Writer;

		protected AbstractLinkedListPageView(Page page) : base(page)
		{
			Reader = new BinaryReader(page);
			Writer = new BinaryWriter(page);
		}

		/// <summary>
		///     The id of the previous <see cref="FreePagesView" /> or 0 if this is the first.
		/// </summary>
		public int PreviousPageId
		{
			get
			{
				Reader.BaseStream.Position = PreviousPageIndexOffset;
				return Reader.ReadInt32();
			}
			set
			{
				Writer.BaseStream.Position = PreviousPageIndexOffset;
				Writer.Write(value);
			}
		}

		/// <summary>
		///     The id of the next <see cref="FreePagesView" /> or 0 if this is the last.
		/// </summary>
		public int NextPageId
		{
			get
			{
				Reader.BaseStream.Position = NextPageIndexOffset;
				return Reader.ReadInt32();
			}
			set
			{
				Writer.BaseStream.Position = NextPageIndexOffset;
				Writer.Write(value);
			}
		}
	}
}