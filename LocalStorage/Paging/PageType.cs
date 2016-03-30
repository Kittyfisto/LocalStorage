namespace LocalStorage.Paging
{
	internal enum PageType : byte
	{
		Invalid = 0,

		StorageDescriptor = 1,
		FreePageIndex = 2,
		TableDescriptor = 3
	}
}