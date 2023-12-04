using Godot;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raele;

public partial class MainSceneNode : Node2D
{
	[Export] private MapGridNode GridNode;
	[Export] private Label UserMessageNode;
	[Export] private BoardSetting Setting;

	private ReadOnlyMainSceneController Controller = null!; // Controller is created on _Ready()

	[Signal] public delegate void ActionCancelledEventHandler();

	public override void _Ready() {
		this.UserMessageNode.Hide();
		this.Controller = MainSceneController.Create(this.Setting);
		this.Controller.GameReadyEvent += this.OnGameReady;
		this.Controller.UnitMovedEvent += this.OnUnitMoved;
		this.Controller.SelectionChangedEvent += this.OnSelectionChanged;
		this.Controller.UnitSelectionRequestEvent += this.OnUnitSelectionRequested;
		this.Controller.TileSelectionRequestEvent += this.OnTileSelectionRequested;
		this.Controller.Start();
	}

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_cancel")) {
			this.EmitSignal(SignalName.ActionCancelled);
		}
    }

    private async Task OnGameReady() {
		this.GridNode.Refresh(this.Controller.Grid);
	}

	private async Task OnUnitMoved(ReadOnlyMainSceneController.UnitMovedEventData eventData) {
		this.GridNode.Refresh(this.Controller.Grid);
	}

	private async Task OnSelectionChanged(UnitInfo? unit) {
		// TODO
	}

	private async Task<UnitInfo> OnUnitSelectionRequested(ReadOnlyMainSceneController.UnitSelectionRequest request) {
		this.UserMessageNode.Text = "Please select a unit.";
		this.UserMessageNode.Show();
		try {
			while (true) {
				TaskCompletionSource<Vector2I> source = new TaskCompletionSource<Vector2I>();
				Task waitUserSelection = this.GridNode.WhenSignalEmitted<Vector2I>(MapGridNode.SignalName.TileClicked)
					.ContinueWith(t => source.TrySetResult(t.Result));
				Task waitUserCancellation = this.WhenSignalEmitted(SignalName.ActionCancelled)
					.ContinueWith(t => source.SetCanceled());
				Vector2I? selectedPosition;
				try {
					selectedPosition = await source.Task;
				} catch(OperationCanceledException e) {
					// this.Soundboard.PlaySE("cancel"); // TODO
					throw e;
				}

				if (this.Controller.Grid.GetUnitAtPosition(selectedPosition.Value, out UnitInfo? unit) && (request.selectionCriteria?.Invoke(unit) ?? true)) {
					// this.Soundboard.PlaySE("accept"); // TODO
					return unit;
				} else {
					// this.Soundboard.PlaySE("reject"); // TODO
				}
			}
		} finally {
			this.UserMessageNode.Hide();
		}
	}

	private async Task<Vector2I> OnTileSelectionRequested(ReadOnlyMainSceneController.TileSelectionRequest request) {
		this.UserMessageNode.Text = "Please select a target.";
		this.UserMessageNode.Show();
		this.GridNode.HighlightPositions(request.ValidPositions);
		try {
			while (true) {
				TaskCompletionSource<Vector2I> source = new TaskCompletionSource<Vector2I>();
				Task waitUserSelection = this.GridNode.WhenSignalEmitted<Vector2I>(MapGridNode.SignalName.TileClicked)
					.ContinueWith(t => source.TrySetResult(t.Result));
				Task waitUserCancellation = this.WhenSignalEmitted(SignalName.ActionCancelled)
					.ContinueWith(t => source.TrySetCanceled());
				Vector2I? selectedPosition;
				try {
					selectedPosition = await source.Task;
				} catch (OperationCanceledException e) {
					// this.Soundboard.PlaySE("cancel"); // TODO
					throw e;
				}

				if (request.ValidPositions.Contains(selectedPosition.Value)) {
					// this.Soundboard.PlaySE("accept"); // TODO
					return selectedPosition.Value;
				} else {
					// this.Soundboard.PlaySE("reject"); // TODO
				}
			}
		} finally {
			this.UserMessageNode.Hide();
			this.GridNode.ResetHighlights();
		}
	}
}
