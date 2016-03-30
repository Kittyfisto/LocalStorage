using System;

namespace LocalStorage.Tables
{
	internal sealed class Column<T>
		: IColumn<T>
	{
		private readonly string _name;

		public Column(string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}

		public Type DataType
		{
			get { return typeof (T); }
		}
	}
}