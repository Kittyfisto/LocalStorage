namespace LocalStorage
{
	public interface IEmbdeddedStorage
	{
		ITableCollection Tables { get; }

		bool IsReadOnly { get; }
	}
}