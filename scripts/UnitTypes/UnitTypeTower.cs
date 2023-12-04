using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypeTower : UnitType {
	public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid) {
		return Enumerable.Range(0, 8)
			.SelectMany(x => Enumerable.Range(0, 8).Select(y => new Vector2I(x, y)))
			.Where(position => position.X == unit.Position.X || position.Y == unit.Position.Y)
			.Where(position => !grid.GetUnitAtPosition(position, out UnitInfo? other) || other.Team != unit.Team)
			.ToArray();
	}
}
