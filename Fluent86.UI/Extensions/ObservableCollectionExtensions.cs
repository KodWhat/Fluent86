using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fluent86.UI.Extensions;

public static class ObservableCollectionExtensions
{
	public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> itemsToAdd)
	{
		foreach (T item in itemsToAdd)
		{
			collection.Add(item);
		}
	}
}
