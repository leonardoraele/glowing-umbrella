using Godot;

namespace Raele;

[GlobalClass]
public partial class UnitSettings : Resource {
	[Export] public Vector2I Position;
	[Export] public UnitType Type;
	[Export] public UnitTeam Team;

}
