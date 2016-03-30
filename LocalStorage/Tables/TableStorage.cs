using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;
using LocalStorage.Strings;
using LocalStorage.Types;

namespace LocalStorage.Tables
{
	internal sealed class TableStorage
		: ITableCollection
	{
		private readonly PageCollection _pages;
		private readonly StringStorage _strings;
		private readonly Dictionary<string, ITable> _tables;
		private readonly TypeStorage _types;

		public TableStorage(PageCollection pages, StringStorage strings, TypeStorage types)
		{
			if (pages == null) throw new ArgumentNullException("pages");
			if (strings == null) throw new ArgumentNullException("strings");
			if (types == null) throw new ArgumentNullException("types");

			_pages = pages;
			_strings = strings;
			_types = types;
			_tables = new Dictionary<string, ITable>();

			foreach (var descriptor in pages.Pages)
			{
				if (descriptor.Type == PageType.TableDescriptor)
				{
					var view = new TableDescriptionView(pages.Load(descriptor));
					var dataType = types.Load(view.DataTypeIndex);
					var tableType = typeof (Table<>).MakeGenericType(dataType);
					var tableName = strings.Load(view.TableNameIndex);

					var count = view.ColumnCount;
					var columns = new List<IColumn>(count);
					for (int i = 0; i < count; ++i)
					{
						var description = view.Columns[i];
						var columnName = _strings.Load(description.NameIndex);
						var columnDataType = _types.Load(description.DataTypeIndex);
						var columnType = typeof (Column<>).MakeGenericType(columnDataType);
						var column = (IColumn) Activator.CreateInstance(columnType, columnName);
						columns.Add(column);
					}

					var table = (ITable)Activator.CreateInstance(tableType, pages, tableName, columns);
					_tables.Add(tableName, table);
				}
			}
		}

		public IEnumerator<ITable> GetEnumerator()
		{
			return _tables.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return _tables.Count; }
		}

		public ITable<T> Add<T>(string tableName)
		{
			if (tableName == null) throw new ArgumentNullException("tableName");

			var properties = GetTableProperties<T>();
			var columnDescriptions = new List<ColumnDescription>(properties.Count);
			var columns = new List<IColumn>(properties.Count);

			// TODO: Find a way to commit all changes in one transaction...
			foreach (PropertyInfo property in properties)
			{
				var columnName = property.Name;
				var columnDataType = property.PropertyType;

				var description = new ColumnDescription
					{
						NameIndex = _strings.Allocate(columnName),
						DataTypeIndex = _types.Allocate(columnDataType)
					};
				columnDescriptions.Add(description);

				var columnType = typeof (Column<>).MakeGenericType(columnDataType);
				var column = (IColumn) Activator.CreateInstance(columnType, columnName);
				columns.Add(column);
			}

			Page tableDescriptionPage = _pages.Allocate(PageType.TableDescriptor);
			var view = new TableDescriptionView(tableDescriptionPage)
				{
					TableNameIndex = _strings.Allocate(tableName),
					DataTypeIndex = _types.Allocate(typeof(T)),
					ColumnCount = columnDescriptions.Count
				};

			// TODO: Account for crossing the page boundary
			for (int i = 0; i < columnDescriptions.Count; ++i)
			{
				view.Columns[i] = columnDescriptions[i];
			}

			view.Commit();

			var table = new Table<T>(_pages, tableName, columns);
			_tables.Add(tableName, table);
			return table;
		}

		/// <summary>
		/// Finds all properties of the given type that are marked as serializable.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private List<PropertyInfo> GetTableProperties<T>()
		{
			PropertyInfo[] allProperties =
				typeof (T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			var tableProperties = new List<PropertyInfo>();
			foreach (var property in allProperties)
			{
				var attribute = property.GetCustomAttribute<DataMemberAttribute>();
				if (attribute != null)
				{
					if (!property.CanRead)
						throw new NotImplementedException();

					if (!property.CanWrite)
						throw new NotImplementedException();

					tableProperties.Add(property);
				}
			}

			return tableProperties;
		}
	}
}