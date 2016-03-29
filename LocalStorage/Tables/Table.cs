using System;
using System.Collections;
using System.Collections.Generic;
using LocalStorage.Paging;

namespace LocalStorage.Tables
{
	internal abstract class Table
	{
		protected readonly TableHeader Header;

		protected Table(TableHeader header)
		{
			Header = header;
		}
	}

	internal sealed class Table<T>
		: Table
		  , ITable<T>
	{
// ReSharper disable StaticFieldInGenericType
		private static readonly Type DataType;
// ReSharper restore StaticFieldInGenericType

		private readonly PageCollection _pages;

		static Table()
		{
			DataType = typeof (T);
		}

		internal Table(PageCollection pages, TableHeader header)
			: base(header)
		{
			_pages = pages;
		}

		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		Type ITable.DataType
		{
			get { return DataType; }
		}

		public string Name
		{
			get { return Header.Name; }
		}

		public void Add(T item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}
	}
}