using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypePawn : UnitType
{
    public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid) {
		// List of valid moves to be filled and returned at the end
        List<Vector2I> validMoves = new List<Vector2I>();

		// Calculates the forward position for referente later
		Vector2I forward = unit.Team == UnitTeam.Player1
			? new Vector2I(unit.Position.X, unit.Position.Y + 1)
			: new Vector2I(unit.Position.X, unit.Position.Y - 1);

		// Checks if simple forward move is possible and add to the list
		if (!grid.CheckPositionIsOccupied(forward)) {
			validMoves.Add(forward);
		}

		// Checks if double start move is possible and adds it to the list
		{
			UnitInfo? other;
			if (unit.Team == UnitTeam.Player1
				&& unit.Position.Y == 1
				&& !grid.CheckPositionIsOccupied(new Vector2I(unit.Position.X, 2))
				&& !grid.CheckPositionIsOccupied(new Vector2I(unit.Position.X, 3))
			) {
				validMoves.Add(new Vector2I(unit.Position.X, 3));
			} else if (unit.Team != UnitTeam.Player1
				&& unit.Position.Y == grid.Height - 2
				&& !grid.CheckPositionIsOccupied(new Vector2I(unit.Position.X, grid.Height - 3))
				&& !grid.CheckPositionIsOccupied(new Vector2I(unit.Position.X, grid.Height - 4))
			) {
				validMoves.Add(new Vector2I(unit.Position.X, grid.Height - 4));
			}
		}

		// Checks which attack moves are valid and add them to the list
		validMoves.AddRange(
			new Vector2I[] {
				forward + Vector2I.Left,
				forward + Vector2I.Right,
			}
				.Where(position => grid.CheckPositionIsInBounds(position))
				.Where(position => grid.GetUnitAtPosition(position, out UnitInfo? other) && other.Team != unit.Team)
		);

		return validMoves.ToArray();
	}

    public override Vector2I[] GetMoveThreatenedPositions(UnitInfo unit, Vector2I position)
		=> new Vector2I[] { position };
}
