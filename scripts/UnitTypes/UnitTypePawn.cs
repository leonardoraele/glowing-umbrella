using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypePawn : UnitType
{
    public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid) {
		Vector2I forward = unit.Team == UnitTeam.Player
			? new Vector2I(unit.Position.X, unit.Position.Y + 1)
			: new Vector2I(unit.Position.X, unit.Position.Y - 1);
		GD.Print("unit.Position:", unit.Position, "unit.Team:", unit.Team, "forward:", forward);
		IEnumerable<Vector2I> forwardMoves = grid.CheckPositionIsOccupied(forward)
			? new Vector2I[] {}
			: new Vector2I[] { forward };
		IEnumerable<Vector2I> attackMoves = new Vector2I[] {
				forward + Vector2I.Left,
				forward + Vector2I.Right,
			}
			.Where(position => grid.CheckPositionIsInBounds(position))
			.Where(position => grid.GetUnitAtPosition(position, out UnitInfo? other) && other.Team != unit.Team);
		return forwardMoves.Concat(attackMoves).ToArray();
	}
}
