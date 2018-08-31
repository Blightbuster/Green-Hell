using System;

namespace Cinemachine
{
	[DocumentationSorting(0f, DocumentationSortingAttribute.Level.Undoc)]
	public sealed class DocumentationSortingAttribute : Attribute
	{
		public DocumentationSortingAttribute(float sortOrder, DocumentationSortingAttribute.Level category)
		{
			this.SortOrder = sortOrder;
			this.Category = category;
		}

		public float SortOrder { get; private set; }

		public DocumentationSortingAttribute.Level Category { get; private set; }

		public enum Level
		{
			Undoc,
			API,
			UserRef
		}
	}
}
