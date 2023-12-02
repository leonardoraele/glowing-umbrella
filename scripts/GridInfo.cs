using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace Raele;

public interface ReadOnlyGridInfo {
	public int Width { get; }
	public int Height { get; }
	public IEnumerable<UnitInfo> Units { get; }
	public bool GetUnitAtPosition(Vector2I position, [NotNullWhen(true)] out UnitInfo? unit);
	public bool GetUnitPosition(UnitInfo unit, [NotNullWhen(true)] out Vector2I? position);
	public bool IsInBounds(Vector2I position);
}

public class GridInfo : ReadOnlyGridInfo {
	public int Width { get; private set; }
	public int Height { get; private set; }

	public IEnumerable<UnitInfo> Units => this.units;

	private List<UnitInfo> units = new List<UnitInfo>();

	public GridInfo(int width = 8, int height = 8)
		=> (Width, Height) = (width, height);

	public bool GetUnitAtPosition(Vector2I position, [NotNullWhen(true)] out UnitInfo? unit) {
		unit = this.units.FirstOrDefault(unit => unit.Position == position);
		return unit != null;
	}

	public bool GetUnitPosition(UnitInfo unit, [NotNullWhen(true)] out Vector2I? position) {
		position = this.units.Contains(unit)
			? unit.Position
			: null;
		return position != null;
	}

	public bool IsInBounds(Vector2I position) {
		return position.X >= 0 && position.X < this.Width
			&& position.Y >= 0 && position.Y < this.Height;
	}

	public void Clear() {
		this.units = new List<UnitInfo>();
	}

	public void AddUnit(UnitInfo unit) {
		if (this.GetUnitAtPosition(unit.Position, out UnitInfo? _)) {
			throw new Exception("Failed to add unit to grid. Cause: Position occupied.");
		}
		this.units.Add(unit);
	}

	public void AddUnit(UnitInfo unit, Vector2I position) {
		unit.Position = position;
		this.AddUnit(unit);
	}

	public void MoveUnit(UnitInfo unit, Vector2I destination) {
		if (!this.units.Contains(unit)) {
			throw new Exception("Failed to move unit. Cause: Unit not on the board.");
		} else if (this.GetUnitAtPosition(destination, out UnitInfo? _)) {
			throw new Exception("Failed to move unit. Cause: Destination position is occupied.");
		}
		unit.Position = destination;
	}
}
