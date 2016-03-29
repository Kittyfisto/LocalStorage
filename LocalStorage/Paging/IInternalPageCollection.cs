using System.Threading.Tasks;

namespace LocalStorage.Paging
{
	internal interface IInternalPageCollection
	{
		Task Write(PageDescriptor descriptor, byte[] data);
		Task Read(PageDescriptor descriptor, byte[] data);
		Page Allocate(int minimumSize);
		void Free(Page page);
	}
}