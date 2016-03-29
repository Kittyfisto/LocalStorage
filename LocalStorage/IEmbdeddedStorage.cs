namespace LocalStorage
{
	public interface IEmbdeddedStorage
	{
		IStorageDescriptor Descriptor { get; }

		ITableCollection Tables { get; }

		bool IsReadOnly { get; }
	}
}