using System.IO;

namespace LocalStorage.Paging.Views
{
	internal sealed class TableDescriptionView
		: AbstractPageView
	{
		private const int ColumnCountOffset = 0;
		private const int TableNameIndexOffset = ColumnCountOffset + sizeof(ushort);
		private const int NextTableDescriptionIdOffset = TableNameIndexOffset + sizeof (int);
		private const int DataTypeIndexOffset = NextTableDescriptionIdOffset + sizeof (int);
		private const int ColumnDescriptionListOffset = DataTypeIndexOffset + sizeof (int);

		private readonly BinaryReader _reader;
		private readonly BinaryWriter _writer;
		private readonly ColumnDescriptionList _columns;

		public TableDescriptionView(Page page)
			: base(page)
		{
			_reader = new BinaryReader(page);
			_writer = new BinaryWriter(page);

			var maximumColumnDescriptionSize = (int)(page.Length - ColumnDescriptionListOffset);
			_columns = new ColumnDescriptionList(_reader, _writer, ColumnDescriptionListOffset, maximumColumnDescriptionSize);
		}

		public int ColumnCount
		{
			get
			{
				_reader.BaseStream.Position = ColumnCountOffset;
				return _reader.ReadUInt16();
			}
			set
			{
				_writer.BaseStream.Position = ColumnCountOffset;
				_writer.Write((ushort) value);
			}
		}

		public int TableNameIndex
		{
			get
			{
				_reader.BaseStream.Position = TableNameIndexOffset;
				return _reader.ReadInt32();
			}
			set
			{
				_writer.BaseStream.Position = TableNameIndexOffset;
				_writer.Write(value);
			}
		}

		public int NextTableDescriptionId
		{
			get
			{
				_reader.BaseStream.Position = NextTableDescriptionIdOffset;
				return _reader.ReadInt32();
			}
			set
			{
				_writer.BaseStream.Position = NextTableDescriptionIdOffset;
				_writer.Write(value);
			}
		}

		public int DataTypeIndex
		{
			get
			{
				_reader.BaseStream.Position = DataTypeIndexOffset;
				return _reader.ReadInt32();
			}
			set
			{
				_writer.BaseStream.Position = DataTypeIndexOffset;
				_writer.Write(value);
			}
		}

		public ColumnDescriptionList Columns
		{
			get { return _columns; }
		}
	}
}