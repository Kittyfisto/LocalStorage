using System;
using System.IO;

namespace LocalStorage.Paging.Views
{
	internal sealed class StorageHeaderView
		: AbstractPageView
		, IStorageHeader
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

		public StorageHeaderView(Page page) : base(page)
		{
			if (page.Descriptor.Type != PageType.StorageHeader)
				throw new ArgumentException("page");

			using (var reader = new BinaryReader(page))
			{
				StorageVersion = reader.ReadInt32();
				CreationTime = reader.ReadDateTime();
			}

		}

		public int StorageVersion { get; set; }

		public DateTime CreationTime { get; set; }

		protected override void Flush()
		{
			using (var writer = new BinaryWriter(Page))
			{
				writer.BaseStream.Position = 0;
				writer.Write(StorageVersion);
				writer.Write(CreationTime);
			}
		}
	}
}