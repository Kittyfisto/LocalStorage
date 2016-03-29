using System;

namespace LocalStorage
{
	public interface IStorageDescriptor
	{
		int StorageVersion { get; }
		DateTime CreationTime { get; }
	}
}