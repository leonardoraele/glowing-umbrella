using System.Linq;
using Godot;

namespace Raele;

public partial class UnitTypeTower : UnitType {
    public override Vector2I[] GetMoveOptions(UnitInfo unit, ReadOnlyGridInfo grid)
        => this.GetAllGridPositions(grid)
            .Where(position => position.X == unit.Position.X || position.Y == unit.Position.Y)
            .Where(position => !grid.GetUnitAtPosition(position, out UnitInfo? other) || other.Team != unit.Team)
            .ToArray();

    public override Vector2I[] GetMoveThreatenedPositions(UnitInfo unit, Vector2I position)
        => new Vector2I[] { position };
}
