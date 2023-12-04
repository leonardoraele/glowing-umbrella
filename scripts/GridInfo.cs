using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace Raele;

public interface ReadOnlyGridInfo {
	public int Width { get; }
	public int Height { get; }
	public List<UnitInfo> Units { get; }
	public bool CheckPositionIsOccupied(Vector2I position);
	public bool GetUnitAtPosition(Vector2I position, [NotNullWhen(true)] out UnitInfo? unit);
	public bool GetUnitPosition(UnitInfo unit, [NotNullWhen(true)] out Vector2I? position);
	public bool CheckPositionIsInBounds(Vector2I position);
}

public class GridInfo : ReadOnlyGridInfo {
	public int Width { get; private set; }
	public int Height { get; private set; }

	public List<UnitInfo> Units { get; private set; } = new List<UnitInfo>();

	public GridInfo(int width = 8, int height = 8)
		=> (Width, Height) = (width, height);

	public bool CheckPositionIsOccupied(Vector2I position) {
		return this.GetUnitAtPosition(position, out UnitInfo? _);
	}

	public bool GetUnitAtPosition(Vector2I position, [NotNullWhen(true)] out UnitInfo? unit) {
		unit = this.Units.FirstOrDefault(unit => unit.Position == position);
		return unit != null;
	}

	public bool GetUnitPosition(UnitInfo unit, [NotNullWhen(true)] out Vector2I? position) {
		position = this.Units.Contains(unit)
			? unit.Position
			: null;
		return position != null;
	}

	public bool CheckPositionIsInBounds(Vector2I position) {
		return position.X >= 0 && position.X < this.Width
			&& position.Y >= 0 && position.Y < this.Height;
	}

	public void Clear() {
		this.Units = new List<UnitInfo>();
	}

	public void AddUnit(UnitInfo unit) {
		if (this.GetUnitAtPosition(unit.Position, out UnitInfo? _)) {
			throw new Exception("Failed to add unit to grid. Cause: Position occupied.");
		}
		this.Units.Add(unit);
	}

	public void AddUnit(UnitInfo unit, Vector2I position) {
		unit.Position = position;
		this.AddUnit(unit);
	}

    public void Remove(UnitInfo unit)
    {
        this.Units.Remove(unit);
    }
}
