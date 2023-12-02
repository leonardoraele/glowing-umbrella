using Godot;

namespace Raele;

public abstract partial class UnitType : Resource {
	[Export] public PackedScene Prefab;
	public abstract Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid);
}
