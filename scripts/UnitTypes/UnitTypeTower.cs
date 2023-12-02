using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypeTower : UnitType {
	public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid) {
		return Enumerable.Empty<Vector2I>()
			.Concat(
				// Right
				Enumerable.Range(1, grid.Width - unit.Position.X)
					.Select(i => new Vector2I(unit.Position.X + i, unit.Position.Y))
			)
			.Concat(
				// Left
				Enumerable.Range(1, unit.Position.X)
					.Select(i => new Vector2I(unit.Position.X - i, unit.Position.Y))
			)
			.Concat(
				// Up
				Enumerable.Range(1, grid.Height - unit.Position.Y)
					.Select(i => new Vector2I(unit.Position.X, unit.Position.Y + i))
			)
			.Concat(
				// Down
				Enumerable.Range(1, unit.Position.Y)
					.Select(i => new Vector2I(unit.Position.X, unit.Position.Y - i))
			)
			.Where(position => !grid.GetUnitAtPosition(position, out _))
			.ToArray();
	}
}
