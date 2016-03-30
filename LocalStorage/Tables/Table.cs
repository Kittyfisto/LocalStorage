using System;
using System.Collections;
using System.Collections.Generic;
using LocalStorage.Paging;

namespace LocalStorage.Tables
{
	internal sealed class Table<T>
		: ITable<T>
	{
// ReSharper disable StaticFieldInGenericType
		private static readonly Type DataType;
// ReSharper restore StaticFieldInGenericType

		private readonly PageStorage _pages;
		private readonly string _tableName;
		private readonly List<IColumn> _columns;

		static Table()
		{
			DataType = typeof (T);
		}

		public Table(PageStorage pages, string tableName, List<IColumn> columns)
		{
			if (pages == null) throw new ArgumentNullException("pages");
			if (tableName == null) throw new ArgumentNullException("tableName");
			if (columns == null) throw new ArgumentNullException("columns");

			_pages = pages;
			_tableName = tableName;
			_columns = columns;
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
			get { return _tableName; }
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

		public IEnumerable<IColumn> Columns
		{
			get { return _columns; }
		}
	}
}