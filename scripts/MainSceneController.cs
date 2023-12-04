using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using static Raele.ReadOnlyMainSceneController;

namespace Raele;

public interface ReadOnlyMainSceneController {
	// PROPERTIES
	public GameState State { get; }
	public ReadOnlyGridInfo Grid { get; }
	public UnitInfo? SelectedUnit { get; }
	public UnitTeam CurrentTurnPlayer { get; }

	// EVENTS
	public event Func<Task>? GameReadyEvent;
	public event Func<UnitMovedEventData, Task>? UnitMovedEvent;
	public event Func<UnitInfo?, UnitInfo?, Task>? SelectionChangedEvent;
	public event Func<UnitSelectionRequest, Task<UnitInfo>>? UnitSelectionRequestEvent;
	public event Func<TileSelectionRequest, Task<Vector2I>>? TileSelectionRequestEvent;
	public event Func<GameEndCondition, Task>? GameEndedEvent;

	// METHODS
	public void Start();

	// DATA ENCAPSULATION TYPES
	public enum GameState {
		/// <summary>
		/// Game controller was created, but Start() was not called yet.
		/// </summary>
		Ready,
		/// <summary>
		/// Game controller is running; player is activelly playing the game at this point.
		/// </summary>
		Started,
		/// <summary>
		/// Game has ended. No more events will be emitted. Create a new controller to start a new game.
		/// </summary>
		Ended,
	}
	public class UnitMovedEventData {
		public UnitInfo Unit { get; }
		public Vector2I Origin { get; }
		public Vector2I Destination { get; }
		public UnitInfo[] ThreatenedUnits { get; init; } = new UnitInfo[0];

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
	public enum GameEndCondition {
		OnlyPlayer1UnitsRemain,
		NoPlayer1UnitsRemain,
	}
}

public partial class MainSceneController : ReadOnlyMainSceneController
{
	// PROPERTIES
    public GameState State { get; private set; } = GameState.Ready;
	public readonly GridInfo grid = new GridInfo();
	public ReadOnlyGridInfo Grid => this.grid;
	public UnitTeam CurrentTurnPlayer { get; private set; } = UnitTeam.Player1;
	public UnitInfo? SelectedUnit { get; private set; } = null;

	// EVENTS
    public event Func<Task>? GameReadyEvent;
	public event Func<UnitMovedEventData, Task>? UnitMovedEvent;
	public event Func<UnitInfo?, UnitInfo?, Task>? SelectionChangedEvent;
	public event Func<UnitSelectionRequest, Task<UnitInfo>>? UnitSelectionRequestEvent;
	public event Func<TileSelectionRequest, Task<Vector2I>>? TileSelectionRequestEvent;
	public event Func<GameEndCondition, Task>? GameEndedEvent;

	// PRIVATES
	private BoardSetting Setting;

	public static ReadOnlyMainSceneController Create(BoardSetting setting) {
		MainSceneController controller = new MainSceneController(setting);
		return controller;
	}

	private MainSceneController(BoardSetting setting)
		=> this.Setting = setting;

	public async void Start() {
		if (this.State != GameState.Ready) {
			throw new Exception("Failed to start MainSceneController. Cause: Game already started.");
		}
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
		Queue<UnitTeam> turnSequence = new Queue<UnitTeam>(new UnitTeam[] {
			UnitTeam.Player1,
			UnitTeam.Player2,
		});
		while (this.State != GameState.Ended) {
			// Process next turn
			try {
				await this.ProcessPlayerTurn(this.CurrentTurnPlayer);
			} catch (Exception e) {
				GD.PushError(e);
			}
			// Checks for end of game
			if (this.CheckIsGameEnded(out GameEndCondition? condition)) {
				try {
					await this.EndGame(condition.Value);
				} catch (Exception e) {
					GD.PushError(e);
				}
			}
			// Moves the turn queue
			turnSequence.Enqueue(turnSequence.Dequeue());
			this.CurrentTurnPlayer = turnSequence.Peek();
		}
	}

	private async Task EndGame(GameEndCondition condition) {
		this.State = GameState.Ended;
		if (this.GameEndedEvent != null) {
			await this.GameEndedEvent(condition);
		}
	}

	public async Task ProcessPlayerTurn(UnitTeam team) {
		GD.Print("Player turn started.");
		if (this.UnitSelectionRequestEvent == null || this.TileSelectionRequestEvent == null) {
			throw new Exception("MainSceneController failed to process player turn. Cause: Required event handlers are not set.");
		}
		while (true) {
			await this.EndSelection();
			UnitInfo? unit;
			try {
				unit = await this.UnitSelectionRequestEvent(new UnitSelectionRequest() {
					selectionCriteria = (UnitInfo unit) => unit.Team == team,
				});
			} catch (OperationCanceledException) {
				continue;
			}
			await this.SetSelection(unit);
			Vector2I[] validPositions = unit.Type.GetMoveOptions(unit, this.Grid);
			Vector2I? destination;
			try {
				destination = await this.TileSelectionRequestEvent(
					new TileSelectionRequest(validPositions)
				);
			} catch(OperationCanceledException) {
				continue;
			}
			await this.PerformMove(unit, destination.Value);
			await this.EndSelection();
			break;
		}
		GD.Print("Player turn ended.");
	}

	public async Task PerformMove(UnitInfo unit, Vector2I destination) {
		GD.Print("Unit moved from position", unit.Position, "to position", destination);

		if (!unit.Type.GetMoveOptions(unit, this.Grid).Contains(destination)) {
			throw new Exception("Failed to move unit. Cause: Invalid movement.");
		}

		UnitInfo[] threatenedUnits = unit.Type.GetMoveThreatenedPositions(unit, destination)
			.SelectMany(position => this.Grid.GetUnitAtPosition(position, out UnitInfo? unit)
				? new UnitInfo[] { unit }
				: new UnitInfo[0]
			)
			.ToArray();

		Vector2I previousPosition = unit.Position;
		threatenedUnits.ForEach(unit => this.grid.Remove(unit));
		unit.Position = destination;

		if (this.UnitMovedEvent != null) {
			await this.UnitMovedEvent(new UnitMovedEventData(unit, previousPosition) {
				ThreatenedUnits = threatenedUnits
			});
		}
	}

	public async Task SetSelection(UnitInfo unit) {
		GD.Print("Unit selected. Unit:", unit);
		UnitInfo? previousSelection = this.SelectedUnit;
		this.SelectedUnit = unit;
		if (this.SelectionChangedEvent != null) {
			await this.SelectionChangedEvent(this.SelectedUnit, previousSelection);
		}
	}

	public async Task EndSelection() {
		UnitInfo? previousSelection = this.SelectedUnit;
		this.SelectedUnit = null;
		if (this.SelectionChangedEvent != null) {
			await this.SelectionChangedEvent(this.SelectedUnit, previousSelection);
		}
	}

	public bool CheckIsGameEnded([NotNullWhen(true)] out GameEndCondition? condition) {
		condition = this.Grid.Units.All(unit => unit.Team == UnitTeam.Player1) ? GameEndCondition.OnlyPlayer1UnitsRemain
			: this.Grid.Units.All(unit => unit.Team != UnitTeam.Player1) ? GameEndCondition.NoPlayer1UnitsRemain
			: null;
		return condition != null;
	}
}
