namespace LocalStorage.Paging
{
	internal enum PageType : byte
	{
		Invalid = 0,

		StorageHeader = 1,
		FreePageIndex = 2,
		StringList = 3,
		TypeList = 4,
		TableDescriptor = 5
	}
}