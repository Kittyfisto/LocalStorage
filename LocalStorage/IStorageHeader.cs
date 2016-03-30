using System;

namespace LocalStorage
{
	public interface IStorageHeader
	{
		int StorageVersion { get; }
		DateTime CreationTime { get; }
	}
}