using Godot;
using System;
using Raele;

namespace Raele;

[Tool]
public partial class MapTileNode : Node2D
{
	[Export] private bool debugEnabled = false;
	[Export] private float WidthPx = 64;
	[Export] private float HeightPx = 8;
	[Export] private string TopColor = "white";
	[Export] private string RightColor = "gray";
	[Export] private string LeftColor = "lightgray";
	[Export] private string OutlineColor = "black";
	[Export] private string HoveredColor = "darkgray";
	[Export] private float OutlineWidth = 1;

	public bool Hovered { get; private set; }

	[Signal] public delegate void TileClickedEventHandler();

	public override void _Draw() {
		this.DrawColoredPolygon(
			new Vector2[] {
				Vector2.Up * this.WidthPx / 4,
				Vector2.Right * this.WidthPx / 2,
				Vector2.Down * this.WidthPx / 4,
				Vector2.Left * this.WidthPx / 2
			},
			Color.FromString(this.TopColor, default)
		);
		this.DrawColoredPolygon(
			new Vector2[] {
				new Vector2(this.WidthPx / 2, 0),
				new Vector2(this.WidthPx / 2, this.HeightPx),
				new Vector2(0, this.WidthPx / 4 + this.HeightPx),
				new Vector2(0, this.WidthPx / 4),
			},
			Color.FromString(this.RightColor, default)
		);
		this.DrawColoredPolygon(
			new Vector2[] {
				new Vector2(this.WidthPx / 2 * -1, 0),
				new Vector2(this.WidthPx / 2 * -1, this.HeightPx),
				new Vector2(0, this.WidthPx / 4 + this.HeightPx),
				new Vector2(0, this.WidthPx / 4),
			},
			Color.FromString(this.LeftColor, default)
		);
		this.DrawPolyline(
			new Vector2[] {
				Vector2.Left * this.WidthPx / 2,
				Vector2.Up * this.WidthPx / 4,
				Vector2.Right * this.WidthPx / 2,
				Vector2.Down * this.WidthPx / 4,
				Vector2.Left * this.WidthPx / 2,
				Vector2.Left * this.WidthPx / 2 + Vector2.Down * this.HeightPx,
				Vector2.Down * (this.WidthPx / 4 + this.HeightPx),
				Vector2.Down * this.WidthPx / 4,
				Vector2.Right * this.WidthPx / 2,
				Vector2.Right * this.WidthPx / 2 + Vector2.Down * this.HeightPx,
			},
			Color.FromString(this.OutlineColor, default),
			this.OutlineWidth
		);
	}

    public override void _Process(double delta)
    {
        this.Modulate = this.Hovered
			? Color.FromString(this.HoveredColor, default)
			: Color.FromString("white", default);
    }

    public override void _Input(InputEvent inputEvent)
    {
		if (inputEvent is InputEventMouseMotion) {
			// Ideally, we would check if the pixel on the mouse position is not transparent, but I couldn't find a way
			// to check this against manually drawn graphics from CanvasItem API, so, instead, we do math to figure out
			// if the mouse is over the tile. This math relies on the shape of the drawn tile.
			Vector2 mouse = this.GetLocalMousePosition();
			this.Hovered = mouse.X >= this.WidthPx / 2 * -1
				&& mouse.X <= this.WidthPx / 2
				&& mouse.Y >= this.WidthPx / 4 * -1 + Mathf.Abs(mouse.X) / 2
				&& mouse.Y <= this.WidthPx / 4 - Mathf.Abs(mouse.X) / 2;
		} else if (this.Hovered && inputEvent is InputEventMouseButton) {
			InputEventMouseButton mouseEvent = inputEvent as InputEventMouseButton ?? throw new Exception("This should never happen.");
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed) {
				this.EmitSignal(SignalName.TileClicked);
			}
		}
    }
}
