using Godot;

namespace Raele;

public partial class MapUnitNode : Node2D
{
    [Export] public Node2D Sprite;

    public bool Selected {
        get => this.Sprite.Position == Vector2.Zero;
        set => this.Sprite.Position = value
            ? Vector2.Up * 10
            : Vector2.Zero;
    }
}
