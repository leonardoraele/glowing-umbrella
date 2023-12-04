using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypeKnight : UnitType
{
    public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid)
		=> new Vector2I[] {
			new Vector2I(unit.Position.X + 2, unit.Position.Y + 1),
			new Vector2I(unit.Position.X + 2, unit.Position.Y - 1),
			new Vector2I(unit.Position.X + 1, unit.Position.Y + 2),
			new Vector2I(unit.Position.X - 1, unit.Position.Y + 2),
			new Vector2I(unit.Position.X - 2, unit.Position.Y + 1),
			new Vector2I(unit.Position.X - 2, unit.Position.Y - 1),
			new Vector2I(unit.Position.X + 1, unit.Position.Y - 2),
			new Vector2I(unit.Position.X - 1, unit.Position.Y - 2),
		}
			.Where(position => grid.CheckPositionIsInBounds(position))
			.Where(position => !grid.GetUnitAtPosition(position, out UnitInfo? other) || other.Team != unit.Team)
			.ToArray();

    public override Vector2I[] GetMoveThreatenedPositions(UnitInfo unit, Vector2I position)
		=> new Vector2I[] { position };
}
