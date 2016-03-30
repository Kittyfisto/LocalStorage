namespace LocalStorage
{
	public interface IEmbdeddedStorage
	{
		IStorageHeader Header { get; }

		ITableCollection Tables { get; }

		bool IsReadOnly { get; }
	}
}