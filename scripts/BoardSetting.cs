using Godot;

namespace Raele;

public partial class BoardSetting : Resource {
	[Export] public Godot.Collections.Array<UnitSettings> Units;
}
