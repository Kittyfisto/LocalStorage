﻿using System;
using System.ComponentModel;
using System.IO;
using FluentAssertions;
using LocalStorage.Paging;
using NUnit.Framework;

namespace LocalStorage.Test.Paging.Views
{
	[TestFixture]
	public abstract class AbstractViewTest
	{
		internal abstract IPageView CreateView(Page page);

		internal abstract PageType PageType { get; }

		[Test]
		[NUnit.Framework.Description("Verifies that the custom view is able to work with all supported page sizes")]
		public void TestCtor1([Values(256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536)] int pageSize)
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, pageSize))
			using (var page = pages.Allocate(PageType))
			{
				IPageView view = null;
				new Action(() => view= CreateView(page)).ShouldNotThrow();
				view.Should().NotBeNull();
			}
		}

		[Test]
		[NUnit.Framework.Description("Verifies that the view throws an exception when a completely invalid pagetype is given")]
		public void TestCtor2()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate((PageType)255))
			{
				new Action(() => CreateView(page)).ShouldThrow<InvalidEnumArgumentException>();
			}
		}

		[Test]
		[NUnit.Framework.Description("Verifies that the view doesn't allow mis-reinterpretation of a page of a different type than the view supports")]
		public void TestCtor3()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate(PageType+1))
			{
				new Action(() => CreateView(page)).ShouldThrow<ArgumentException>();
			}
		}

		[Test]
		[NUnit.Framework.Description("Verifies that the view doesn't allow construction from a null page")]
		public void TestCtor4()
		{
			new Action(() => CreateView(null)).ShouldThrow<ArgumentNullException>();
		}
	}
}