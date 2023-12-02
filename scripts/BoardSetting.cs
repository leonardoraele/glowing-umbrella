using Godot;

namespace Raele;

public partial class BoardSetting : Resource {
	[Export] public Vector2I Position { get; private set; }
	[Export] public UnitType Type { get; private set; }
}
