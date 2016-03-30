using System;
using LocalStorage.Paging;
using LocalStorage.Strings;

namespace LocalStorage.Types
{
	internal sealed class TypeStorage
	{
		private readonly PageStorage _pages;
		private readonly StringStorage _strings;

		public TypeStorage(PageStorage pages, StringStorage strings)
		{
			if (pages == null) throw new ArgumentNullException("pages");
			if (strings == null) throw new ArgumentNullException("strings");

			_pages = pages;
			_strings = strings;
		}

		public int Allocate(Type type)
		{
			return _strings.Add(type.AssemblyQualifiedName);
		}

		public Type Load(int dataTypeIndex)
		{
			var name = _strings.Load(dataTypeIndex);
			// TODO: Future extension point
			return Type.GetType(name);
		}
	}
}