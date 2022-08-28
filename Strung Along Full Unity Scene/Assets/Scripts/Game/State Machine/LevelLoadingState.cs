using UnityEngine;

public class LevelLoadingState : LevelBaseState
{
	
	float _loadProgress = 0.0f;
	
	public override void EnterState(GameStateManager ctx) {
		Debug.Log("Level Loading...");
		_loadProgress = 0.0f;
		
		SetStrings(ctx._p1Anchor, 4.0f, false);
		SetStrings(ctx._p2Anchor, 4.0f, false);
		
		ctx._currentLevel.FreezeAll();
	}
	
	public override void UpdateState(GameStateManager ctx) {
		

		// move new props onto stage
		foreach (StageProp prop in ctx._currentLevel._props) {
			prop.gameObject.SetActive(true);
			MovePropOn(prop);
		}

		// move player anchors
		MoveAnchor(ctx._p1Anchor, ctx._currentLevel._p1Spawn.GetComponent<StageProp>().originalPosition);
		MoveAnchor(ctx._p2Anchor, ctx._currentLevel._p2Spawn.GetComponent<StageProp>().originalPosition);
		
		if (_loadProgress >= 1.0f) {
			// all props are in position!
			ctx._currentLevel.UnfreezeAll();
			SetStrings(ctx._p1Anchor, ctx._currentLevel._p1Spawn._stringLength, ctx._currentLevel._p1Spawn._elasticString);
			SetStrings(ctx._p2Anchor, ctx._currentLevel._p2Spawn._stringLength, ctx._currentLevel._p2Spawn._elasticString);
			
			if (ctx._currentLevel == ctx._database.GetFirstLevel() ) {
				// it's the first level: play the intro cinematic.
				ctx.SwitchState(ctx.FirstPlayingState);
			} else {
				// otherwise just go to next level.
				ctx.SwitchState(ctx.PlayingState);
			}
		} else {
			_loadProgress += (Time.deltaTime / ctx._loadTime);
		}
		
	}
	
	void SetStrings(StringRoot anchor, float length, bool elastic) {
		anchor.stringLength = length;
		anchor.elasticString = elastic;
	}
	
	void MoveAnchor(StringRoot anchor, Vector3 newPos) {
		
		anchor.transform.position = Vector3.Lerp(anchor.transform.position, newPos, _loadProgress);
		
	}
	
	void MovePropOn(StageProp prop) {
		
		Vector3 offstagePosition = Vector3.zero;
		switch (prop.stageMoveDirection) {
			case GameStateManager.Direction.Top:
				offstagePosition = prop.originalPosition + new Vector3(0, GameStateManager.TOP_BOUNDARY, 0);
				break;
			case GameStateManager.Direction.Left:
				offstagePosition = prop.originalPosition + new Vector3(GameStateManager.LEFT_BOUNDARY, 0, 0);
				break;
			case GameStateManager.Direction.Right:
				offstagePosition = prop.originalPosition + new Vector3(GameStateManager.RIGHT_BOUNDARY, 0, 0);
				break;
			case GameStateManager.Direction.Bottom:
				offstagePosition = prop.originalPosition + new Vector3(0, GameStateManager.BOTTOM_BOUNDARY, 0);
				break;
		}
		
		prop.transform.position = Vector3.Lerp(offstagePosition, prop.originalPosition, _loadProgress);
		
		
	}
	
}
