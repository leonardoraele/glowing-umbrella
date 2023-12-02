using Godot;
using System;
using System.Threading.Tasks;

namespace Raele;

public class PlayerTurnPhase : TurnPhase {

    public MainSceneController.ITurnController controller { get; private set; }

	public PlayerTurnPhase(MainSceneController.ITurnController controller) {
		this.controller = controller;
	}

    public async Task Begin() {
		await this.controller.SceneController.EndSelection();
		while (this.controller.SceneController.SelectedUnit == null) {
			UnitInfo unit = await this.controller.RequestUnitSelection(new MainSceneController.UnitSelectionRequest() {
				selectionCriteria = (UnitInfo unit) => unit.Team == UnitTeam.Player,
			});
			await this.controller.SceneController.SetSelection(unit);
			try {
                Vector2I[] validPositions = unit.Type.GetMoveOptions(unit, this.controller.SceneController.Grid);
				Vector2I destination = await this.controller.RequestTileSelection(
					new MainSceneController.TileSelectionRequest(validPositions)
				);
				await this.controller.SceneController.PerformMove(unit, destination);
			} catch(OperationCanceledException e) {
				// Do nothing
			} finally {
				await this.controller.SceneController.EndSelection();
			}
		}
		GD.Print("Player turn ends.");
    }
}
