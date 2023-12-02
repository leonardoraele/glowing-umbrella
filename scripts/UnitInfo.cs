using Godot;

namespace Raele;

public class UnitInfo {
	public UnitTeam Team = UnitTeam.Player;
	public Vector2I Position = Vector2I.Zero;
	public UnitType Type;

	public UnitInfo(UnitType type)
		=> this.Type = type;
}
