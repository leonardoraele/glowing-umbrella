using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypeQueen : UnitType
{
    public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid)
		=> this.GetAllGridPositions(grid)
            .Where(position => position.X == unit.Position.X
				|| position.Y == unit.Position.Y
				|| position.X + position.Y == unit.Position.X + unit.Position.Y
				|| unit.Position.X - position.X == unit.Position.Y - position.Y
			)
            .Where(position => !grid.GetUnitAtPosition(position, out UnitInfo? other) || other.Team != unit.Team)
            .ToArray();
}
