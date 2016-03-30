using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LocalStorage.Paging.Views
{
	internal sealed class ColumnDescriptionList
		: IEnumerable<ColumnDescription>
	{
		private readonly BinaryReader _reader;
		private readonly BinaryWriter _writer;
		private readonly int _offset;
		private readonly int _length;
		public readonly int Length;

		public const int ColumnDescriptionSize = sizeof (int)*2;

		public ColumnDescriptionList(BinaryReader reader, BinaryWriter writer, int offset, int length)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			if (writer == null) throw new ArgumentNullException("writer");

			_reader = reader;
			_writer = writer;
			_offset = offset;
			_length = length;
			Length = _length/ColumnDescriptionSize;
		}

		public ColumnDescription this[int index]
		{
			get
			{
				if (index < 0 || index >= Length)
					throw new IndexOutOfRangeException();

				_reader.BaseStream.Position = _offset + index*ColumnDescriptionSize;
				return new ColumnDescription
				{
					DataTypeIndex = _reader.ReadInt32(),
					NameIndex = _reader.ReadInt32()
				};
			}
			set
			{
				if (index < 0 || index >= Length)
					throw new IndexOutOfRangeException();

				_writer.BaseStream.Position = _offset + index*ColumnDescriptionSize;
				_writer.Write(value.DataTypeIndex);
				_writer.Write(value.NameIndex);
			}
		}

		public IEnumerator<ColumnDescription> GetEnumerator()
		{
			for (int i = 0; i < Length; ++i)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}