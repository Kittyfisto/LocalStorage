using System;
using LocalStorage.Paging;

namespace LocalStorage.Tables
{
	internal struct TableHeader
	{
		public string DataType;
		public string Name;

		public TableHeader(string name, Type dataType)
		{
			Name = name;
			DataType = dataType.AssemblyQualifiedName;
		}

		public int Size
		{
			get { return 1024; }
		}

		public void WriteTo(Page page)
		{
			page.Writer.Write(Name);
			page.Writer.Write(DataType);
		}
	}
}