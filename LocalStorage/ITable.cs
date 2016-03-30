using System;
using System.Collections;
using System.Collections.Generic;

namespace LocalStorage
{
	public interface ITable<T>
		: ITable
		  , ICollection<T>
	{
	}

	public interface ITable
		: IEnumerable
	{
		/// <summary>
		///     The data-type being stored in this table.
		/// </summary>
		Type DataType { get; }

		IEnumerable<IColumn> Columns { get; }

		string Name { get; }
	}
}