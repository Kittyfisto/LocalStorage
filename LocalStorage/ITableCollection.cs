using System.Collections.Generic;

namespace LocalStorage
{
	public interface ITableCollection
		: IEnumerable<ITable>
	{
		int Count { get; }

		ITable<T> Add<T>(string tableName);
	}
}