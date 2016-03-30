using System;
using System.ComponentModel;

namespace LocalStorage.Paging.Views
{
	/// <summary>
	/// Base class for all <see cref="IPageView"/>s that implements the commonly expected funationality.
	/// </summary>
	internal abstract class AbstractPageView
		: IPageView
	{
		protected readonly Page Page;

		protected AbstractPageView(Page page)
		{
			if (page == null) throw new ArgumentNullException("page");
			if (!Enum.IsDefined(typeof(PageType), page.Descriptor.Type))
				throw new InvalidEnumArgumentException("page", (int) page.Descriptor.Type, typeof (PageType));

			Page = page;
		}

		public void Commit()
		{
			Flush();
			Page.Commit();
		}

		protected virtual void Flush()
		{}
	}
}