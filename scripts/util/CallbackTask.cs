using System;
using System.Threading.Tasks;

public static class CallbackTask {
	public static Task<T> Create<T>(Action<Action<T>> executioner) {
		return Task.Run(() => {
			TaskCompletionSource<T> source = new TaskCompletionSource<T>();
			Action<T> resolve = (T t) => source.TrySetResult(t);
			executioner(resolve);
			return source.Task;
		});
	}
}
