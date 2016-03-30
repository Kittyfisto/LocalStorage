using System.Runtime.Serialization;

namespace LocalStorage.Test.Classes
{
	[DataContract]
	public sealed class IntStringType
	{
		[DataMember]
		public int Index { get; set; }

		[DataMember]
		public string Value { get; set; }
	}
}