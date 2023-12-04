using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele;

public abstract partial class UnitType : Resource {
	[Export] public PackedScene Prefab;
	public abstract Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid);
	protected IEnumerable<Vector2I> GetAllGridPositions(ReadOnlyGridInfo grid) => Enumerable.Range(0, grid.Width)
			.SelectMany(x => Enumerable.Range(0, grid.Height).Select(y => new Vector2I(x, y)));
}
