using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raele;

public partial class MainSceneController
{
	public readonly GridInfo Grid = new GridInfo();
	public TurnPhase? TurnPhase;
	public TurnPhase? NextTurnPhase;

	public UnitInfo? SelectedUnit { get; private set; } = null;

	public event Func<UnitMovedEventData, Task>? UnitMovedEvent;
	public event Func<UnitInfo?, Task>? SelectionChangedEvent;
	public event Func<UnitSelectionRequest, Task<UnitInfo>>? UnitSelectionRequestEvent;
	public event Func<TileSelectionRequest, Task<Vector2I>>? TileSelectionRequestEvent;

	public class UnitMovedEventData {
		UnitInfo Unit;
		Vector2I Origin;
		Vector2I Destination;

		public UnitMovedEventData(UnitInfo unit, Vector2I origin, Vector2I? destination = null)
			=> (Unit, Origin, Destination) = (unit, origin, destination ?? unit.Position);
	}

	public class UnitSelectionRequest {
		public Func<UnitInfo, bool>? selectionCriteria;
	}

	public class TileSelectionRequest {
		public Vector2I[] ValidPositions;
		public Vector2I[]? DangerHighlightPositions;

		public TileSelectionRequest(Vector2I[] validPositions)
			=> ValidPositions = validPositions;
	}

	public interface ITurnController {
		public MainSceneController SceneController { get; }
		public Task<UnitInfo> RequestUnitSelection(UnitSelectionRequest request);
		public Task<Vector2I> RequestTileSelection(TileSelectionRequest request);
	}

	private class TurnController : ITurnController {
		public MainSceneController SceneController { get; private set; }
		public TurnController(MainSceneController controller)
			=> SceneController = controller;
		public void ChangePhase(TurnPhase TurnPhase)
			=> this.SceneController.TurnPhase = TurnPhase;
		public Task<UnitInfo> RequestUnitSelection(UnitSelectionRequest request)
			=> this.SceneController.UnitSelectionRequestEvent?.Invoke(request)
			?? Task.FromCanceled<UnitInfo>(CancellationToken.None);
		public Task<Vector2I> RequestTileSelection(TileSelectionRequest request)
			=> this.SceneController.TileSelectionRequestEvent?.Invoke(request)
			?? Task.FromCanceled<Vector2I>(CancellationToken.None);
	}

	public MainSceneController() {
		this.NextTurnPhase = new PlayerTurnPhase(new TurnController(this));
	}

	public async void Begin() {
		while (this.NextTurnPhase != null) {
			this.TurnPhase = this.NextTurnPhase;
			this.NextTurnPhase = null;
			try {
				await this.TurnPhase.Begin();
			} catch(Exception e) {
				GD.PushError(e);
				// TODO Probably should emit some event like "Reset" so that the UI can react
			}
		}
	}

	public void Setup(BoardSetting setting) {
		this.Grid.Clear();
		this.Grid.AddUnit(new UnitInfo(setting.Type), setting.Position);
	}

	public async Task PerformMove(UnitInfo unit, Vector2I destination) {
		GD.Print("Unit moved from position", unit.Position, "to position", destination);
		Vector2I previousPosition = unit.Position;
		this.Grid.MoveUnit(unit, destination);
		if (this.UnitMovedEvent != null) {
			await this.UnitMovedEvent(new UnitMovedEventData(unit, previousPosition));
		}
	}

	public async Task SetSelection(UnitInfo unit) {
		this.SelectedUnit = unit;
		if (this.SelectionChangedEvent != null) {
			await this.SelectionChangedEvent(this.SelectedUnit);
		}
	}

	public async Task EndSelection() {
		this.SelectedUnit = null;
		if (this.SelectionChangedEvent != null) {
			await this.SelectionChangedEvent(this.SelectedUnit);
		}
	}
}
