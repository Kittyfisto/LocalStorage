using System.Collections.Generic;

namespace LocalStorage
{
	public interface ITableCollection
		: IEnumerable<ITable>
	{
		int Count { get; }

		ITable Add<T>(string tableName);
	}
}