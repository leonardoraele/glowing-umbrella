using Godot;
using System;
using System.Threading.Tasks;

namespace Raele;

public interface ReadOnlyMainSceneController {
	// PROPERTIES
	public ReadOnlyGridInfo Grid { get; }
	public UnitInfo? SelectedUnit { get; }

	// EVENTS
	public event Func<Task>? GameReadyEvent;
	public event Func<UnitMovedEventData, Task>? UnitMovedEvent;
	public event Func<UnitInfo?, Task>? SelectionChangedEvent;
	public event Func<UnitSelectionRequest, Task<UnitInfo>>? UnitSelectionRequestEvent;
	public event Func<TileSelectionRequest, Task<Vector2I>>? TileSelectionRequestEvent;

	// METHODS
	public void Start();

	// DATA ENCAPSULATION TYPES
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
}

public partial class MainSceneController : ReadOnlyMainSceneController
{
	public readonly GridInfo grid = new GridInfo();
	public ReadOnlyGridInfo Grid => this.grid;

	public UnitInfo? SelectedUnit { get; private set; } = null;

	public event Func<Task>? GameReadyEvent;
	public event Func<ReadOnlyMainSceneController.UnitMovedEventData, Task>? UnitMovedEvent;
	public event Func<UnitInfo?, Task>? SelectionChangedEvent;
	public event Func<ReadOnlyMainSceneController.UnitSelectionRequest, Task<UnitInfo>>? UnitSelectionRequestEvent;
	public event Func<ReadOnlyMainSceneController.TileSelectionRequest, Task<Vector2I>>? TileSelectionRequestEvent;

	private BoardSetting Setting;

	public static ReadOnlyMainSceneController Create(BoardSetting setting) {
		MainSceneController controller = new MainSceneController(setting);
		return controller;
	}

	private MainSceneController(BoardSetting setting)
		=> this.Setting = setting;

	public async void Start() {
		this.Setup();
		if (this.GameReadyEvent != null) {
			await this.GameReadyEvent.Invoke();
		}
		await this.ProcessMainLoop();
	}

	public void Setup() {
		this.Setting.Units.ForEach(unit =>
			this.grid.AddUnit(new UnitInfo(unit.Type) { Team = unit.Team }, unit.Position)
		);
	}

	public async Task ProcessMainLoop() {
		while (true) {
			await this.ProcessPlayerTurn();
		}
	}

	public async Task ProcessPlayerTurn() {
		GD.Print("Player turn started.");
		while (true) {
			await this.EndSelection();
			UnitInfo? unit;
			try {
				unit = await this.UnitSelectionRequestEvent(new ReadOnlyMainSceneController.UnitSelectionRequest() {
					selectionCriteria = (UnitInfo unit) => unit.Team == UnitTeam.Player,
				});
			} catch (OperationCanceledException e) {
				continue;
			}
			await this.SetSelection(unit);
			Vector2I[] validPositions = unit.Type.GetMoveOptions(unit, this.Grid);
			Vector2I? destination;
			try {
				destination = await this.TileSelectionRequestEvent(
					new ReadOnlyMainSceneController.TileSelectionRequest(validPositions)
				);
			} catch(OperationCanceledException e) {
				continue;
			}
			await this.PerformMove(unit, destination.Value);
			break;
		}
		GD.Print("Player turn ended.");
	}

	public async Task PerformMove(UnitInfo unit, Vector2I destination) {
		GD.Print("Unit moved from position", unit.Position, "to position", destination);
		Vector2I previousPosition = unit.Position;
		this.grid.MoveUnit(unit, destination);
		if (this.UnitMovedEvent != null) {
			await this.UnitMovedEvent(new ReadOnlyMainSceneController.UnitMovedEventData(unit, previousPosition));
		}
	}

	public async Task SetSelection(UnitInfo unit) {
		GD.Print("Unit selected. Unit:", unit);
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
