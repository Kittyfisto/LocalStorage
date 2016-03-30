using LocalStorage.Strings;
using LocalStorage.Types;

namespace LocalStorage.Paging.Views
{
	internal struct ColumnDescription
	{
		/// <summary>
		/// The index of the column's name in the <see cref="StringStorage"/>.
		/// </summary>
		public int NameIndex;

		/// <summary>
		/// The index of the column's data type in the <see cref="TypeStorage"/>.
		/// </summary>
		public int DataTypeIndex;
	}
}