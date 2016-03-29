using System;
using System.IO;
using LocalStorage.Paging;

namespace LocalStorage
{
	internal sealed class StorageDescriptor
		: IStorageDescriptor
	{
		/// <summary>
		/// The current version of the storage.
		/// Increment when necessary.
		/// </summary>
		/// <remarks>
		/// History:
		/// 
		/// Version 1: 29.03.2016 - XXX
		/// </remarks>
		public const int CurrentStorageVersion = 1;

		public int StorageVersion;
		public DateTime CreationTime;

		public const int HeaderSize = sizeof(int) + sizeof(long) + sizeof(byte);

		public void WriteTo(Page page)
		{
			using (var writer = new BinaryWriter(page))
			{
				writer.Write(StorageVersion);
				writer.Write(CreationTime);
			}
		}

		public void ReadFrom(Page page)
		{
			using (var reader = new BinaryReader(page))
			{
				StorageVersion = reader.ReadInt32();
				CreationTime = reader.ReadDateTime();
			}
		}

		int IStorageDescriptor.StorageVersion
		{
			get { return StorageVersion; }
		}

		DateTime IStorageDescriptor.CreationTime
		{
			get { return CreationTime; }
		}
	}
}