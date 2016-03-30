namespace LocalStorage.Paging
{
	public interface IPageView
	{
		/// <summary>
		/// Commits the view and the page to the underlying stream (e.g. the file or whatever).
		/// </summary>
		void Commit();
	}
}