using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypeKing : UnitType
{
    public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid)
		=> new Vector2I[] {
			unit.Position + Vector2I.Right,
			unit.Position + Vector2I.Up,
			unit.Position + Vector2I.Down,
			unit.Position + Vector2I.Left,
		}
			.Where(position => grid.CheckPositionIsInBounds(position))
            .Where(position => !grid.GetUnitAtPosition(position, out UnitInfo? other) || other.Team != unit.Team)
            .ToArray();
}
