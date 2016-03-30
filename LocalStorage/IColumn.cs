using System;

namespace LocalStorage
{
	public interface IColumn
	{
		string Name { get; }
		Type DataType { get; }
	}

	public interface IColumn<T>
		: IColumn
	{
		
	}
}