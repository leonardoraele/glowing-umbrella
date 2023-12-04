using BidirectionalMap;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Raele;

[Tool]
public partial class MapGridNode : Node2D
{
	[Export] private PackedScene MapTilePrefab;
	[Signal] public delegate void TileClickedEventHandler(Vector2I position);

	private MapTileNode[] Tiles = new MapTileNode[0];
	private BiMap<UnitInfo, MapUnitNode> UnitMap = new BiMap<UnitInfo, MapUnitNode>();
	private ReadOnlyGridInfo Grid = new GridInfo();

	public override void _Ready() {
		this.Refresh(this.Grid);
	}

	private static int GetIndex(Vector2I position, int width) {
		return position.Y * width + position.X;
	}

	private int GetIndex(Vector2I position) {
		return MapGridNode.GetIndex(position, this.Grid.Width);
	}

	private void SetTile(Vector2I position, MapTileNode tile) {
		this.Tiles[this.GetIndex(position)] = tile;
	}

	public bool GetTile(Vector2I position, [NotNullWhen(true)] out MapTileNode? tile) {
		int index = this.GetIndex(position);
		if (index >= 0 && index < this.Tiles.Length) {
			tile = this.Tiles[index];
			return true;
		}
		tile = null;
		return false;
	}

	public bool GetUnitNode(UnitInfo unit, [NotNullWhen(true)] out MapUnitNode? node) {
		node = this.UnitMap.Forward[unit];
		return node != null;
	}

	public static int GetTileZIndex(Vector2I position) {
		return (position.X - position.Y) * 10;
	}

	public static int GetUnitZIndex(Vector2I position) {
		return MapGridNode.GetTileZIndex(position) + 5;
	}

	private void OnTileClicked(Vector2I position) {
		this.EmitSignal(SignalName.TileClicked, position);
	}

	public void Refresh(ReadOnlyGridInfo newGrid) {
		this.RefreshTiles(newGrid);
		this.RefreshUnits(newGrid);
		this.Grid = newGrid;
	}

	public void RefreshTiles(ReadOnlyGridInfo newGrid) {
		// Fill new tiles array by tranferring old tiles to the new array and creating new tiles when necessary (if the
		// new grid is larger than the previous one)
		MapTileNode[] newTiles = new MapTileNode[newGrid.Width * newGrid.Height];
		for (int y = 0; y < newGrid.Height; y++) {
			for (int x = 0; x < newGrid.Width; x++) {
				Vector2I position = new Vector2I(x, y);
				int index = MapGridNode.GetIndex(position, newGrid.Width);
				if (this.GetTile(position, out MapTileNode? tile)) {
					newTiles[index] = tile;
				} else {
					MapTileNode newTile = this.CreateTile(position);
					newTiles[index] = newTile;
					this.AddChild(newTile);
				}
			}
		}

		// Free tiles that were not moved to the new tiles array
		HashSet<MapTileNode> set = new HashSet<MapTileNode>(newTiles);
		this.Tiles.AsEnumerable()
			.Where(tile => !set.Contains(tile))
			.ForEach(tile => tile.QueueFree());

		// Update fields
		this.Tiles = newTiles;
	}

	private MapTileNode CreateTile(Vector2I position) {
		MapTileNode tile = this.MapTilePrefab.Instantiate<MapTileNode>();
		tile.Position = new Vector2(position.X * 32 + position.Y * 32, position.X * 16 + position.Y * -16);
		tile.Name = $"{typeof(MapTileNode).Name} [{position.X}, {position.Y}]";
		tile.ZIndex = MapGridNode.GetTileZIndex(position);
		tile.TileClicked += () => this.OnTileClicked(position);
		return tile;
	}

	public void RefreshUnits(ReadOnlyGridInfo newGrid) {
		// Remove units not in the grid anymore
		this.UnitMap.AsEnumerable()
			.Where(pair => !newGrid.GetUnitPosition(pair.Key, out _))
			.ForEach(pair => this.RemoveUnit(pair.Key));

		// Update units already in the grid
		this.UnitMap.ForEach(pair => {
			if (this.GetTile(pair.Key.Position, out MapTileNode? tile)) {
				pair.Value.Position = tile.Position;
				pair.Value.ZIndex = GetUnitZIndex(pair.Key.Position);
			}
		});

		// Add new units
		newGrid.Units.Where(unit => !this.UnitMap.Forward.ContainsKey(unit))
			.ForEach(unit => {
				try {
					this.AddUnit(unit);
				} catch(Exception e) {
					GD.PushError(e);
				}
			});
	}

	public void RemoveUnit(UnitInfo unit) {
		this.UnitMap.Forward[unit].QueueFree();
		this.UnitMap.Remove(unit);
	}

	public void AddUnit(UnitInfo unit) {
		if (unit.Type.Prefab == null) {
			GD.PushError(
				"Failed to add unit. The Resource file for this unit type doesn't have a Prefab setup.",
				"Type:", unit.Type.GetType().Name
			);
			return;
		}
		if (!this.GetTile(unit.Position, out MapTileNode? tile)) {
			GD.PushError(
				"Failed to add unit to MapGridNode. Unit is in a invalid position for this grid.",
				"Position:", unit.Position,
				"Grid Broundaries:", new Vector2I(this.Grid.Width, this.Grid.Height)
			);
			return;
		}
		MapUnitNode? node;
		try {
			node = unit.Type.Prefab.Instantiate<MapUnitNode>();
		} catch (InvalidCastException e) {
			GD.PushError("Failed to add unit to MapGridNode. Cause: Prefab for unit type is not a MapUnitNode.");
			throw e;
		}
		node.Position = tile.Position;
		node.ZIndex = MapGridNode.GetUnitZIndex(unit.Position);
		this.AddChild(node);
		this.UnitMap.Add(unit, node);
	}

    public void HighlightPositions(Vector2I[] positions)
		=> positions.Select(position => this.GetTile(position, out MapTileNode? tile) ? tile : null)
			.Where(tile => tile != null)
			.ForEach(tile => tile!.Highlighted = true);

    public void ResetHighlights()
		=> this.Tiles.ForEach(tile => tile.Highlighted = false);
}
