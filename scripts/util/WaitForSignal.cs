using System.Threading.Tasks;
using Godot;

// Note: For the overloads that return tuples, you are supposed to deconstruct them, like this:
// (int a, int b, int c) = await node.WaitForSignal(NodeType.SignalName.MySignal)
public static class WaitForSignalExtensionMethod {
	public static async Task WhenSignalEmitted(this GodotObject subject, StringName signalName) {
        await subject.ToSignal(subject, signalName);
	}
	public static async Task<T> WhenSignalEmitted<[MustBeVariant] T>(this GodotObject subject, StringName signalName) {
        Variant[] result = await subject.ToSignal(subject, signalName);
		T t = result[0].As<T>();
		return t;
	}
	public static async Task<(T0, T1)> WhenSignalEmitted<[MustBeVariant] T0, [MustBeVariant] T1>(this GodotObject subject, StringName signalName) {
        Variant[] result = await subject.ToSignal(subject, signalName);
		T0 t0 = result[0].As<T0>();
		T1 t1 = result[0].As<T1>();
		return (t0, t1);
	}
	public static async Task<(T0, T1, T2)> WhenSignalEmitted<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(this GodotObject subject, StringName signalName) {
        Variant[] result = await subject.ToSignal(subject, signalName);
		T0 t0 = result[0].As<T0>();
		T1 t1 = result[0].As<T1>();
		T2 t2 = result[0].As<T2>();
		return (t0, t1, t2);
	}
	public static async Task<(T0, T1, T2, T3)> WhenSignalEmitted<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(this GodotObject subject, StringName signalName) {
        Variant[] result = await subject.ToSignal(subject, signalName);
		T0 t0 = result[0].As<T0>();
		T1 t1 = result[0].As<T1>();
		T2 t2 = result[0].As<T2>();
		T3 t3 = result[0].As<T3>();
		return (t0, t1, t2, t3);
	}
}
