using UnityEngine;

public class LevelUnloadingState : GameState
{
	
	float _unloadProgress = 0.0f;
	
	public override void EnterState(GameStateManager ctx) {
		Debug.Log("Clearing the Stage...");
		_unloadProgress = 0.0f;
		
		ctx._currentLevel.ToggleColliders(false);
	}
	
	public override void UpdateState(GameStateManager ctx) {
		
		// move props off the stage
		foreach (StageProp prop in ctx._currentLevel._props) {
			MovePropOff(prop);
		}
		
		if (_unloadProgress >= 1.0f) {
			// the stage is all clear!
			
			foreach (StageProp prop in ctx._currentLevel._props) {
				prop.gameObject.SetActive(false);
			}
			
			ctx._currentLevel.ToggleColliders(true);
			ctx.SwitchState(ctx.EmptyState);
		} else {
			_unloadProgress += (Time.deltaTime / ctx._unloadTime);
		}
		
		
	}
	
	void MovePropOff(StageProp prop) {
		
		Vector3 offstagePosition = Vector3.zero;
		switch (prop.stageMoveDirection) {
			case GameStateManager.Direction.Top:
				offstagePosition = prop.originalPosition + new Vector3(0, GameStateManager.TOP_BOUNDARY, 0);
				break;
			case GameStateManager.Direction.Left:
			case GameStateManager.Direction.Scrolling:
				offstagePosition = prop.originalPosition - new Vector3(GameStateManager.SIDE_BOUNDARY, 0, 0);
				break;
			case GameStateManager.Direction.Right:
				offstagePosition = prop.originalPosition + new Vector3(GameStateManager.SIDE_BOUNDARY, 0, 0);
				break;
			case GameStateManager.Direction.Bottom:
				offstagePosition = prop.originalPosition + new Vector3(0, GameStateManager.BOTTOM_BOUNDARY, 0);
				break;
		}
		
		prop.transform.position = Vector3.Lerp(prop.originalPosition, offstagePosition, _unloadProgress);
		
		
	}
}
