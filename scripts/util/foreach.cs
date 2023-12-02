using System;
using System.Collections.Generic;

namespace Raele;

public static class ForEachExtensionMethod {
	public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
	{
		foreach(T item in sequence) {
			action(item);
		}
	}
}
