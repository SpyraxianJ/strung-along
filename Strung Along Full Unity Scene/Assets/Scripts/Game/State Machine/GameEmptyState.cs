using UnityEngine;

public class GameEmptyState : GameState
{
	public override void EnterState(GameStateManager ctx) {
		Debug.Log("Stage is clear!");
		
		CheckFlags(ctx);
	}
	
	public override void UpdateState(GameStateManager ctx) {
		// in this state we sort out what level to load next. in most cases
		// just check which level is next in line and load that one!
		
		if (ctx._nextLevelToLoad != null) {
			ctx._currentLevel = ctx._nextLevelToLoad;
			ctx._nextLevelToLoad = ctx._database.GetLevelAfter(ctx._currentLevel);
			if (ctx._nextLevelToLoad != null) {
				ctx.SwitchState(ctx.LoadingState);
			} else {
				ctx._nextLevelToLoad = ctx._database.GetActAfter(ctx._currentLevel);
			
				if (ctx._nextLevelToLoad != null) {
					ctx._actChange = true;
					ctx.SwitchState(ctx.LoadingState);
				} else {
					ctx._gameEnd = true;
					ctx.SwitchState(ctx.LoadingState);
				}
			}
		}
		
	}
	
	void CheckFlags(GameStateManager ctx) {
		
		if (ctx._playIntro) {
			ctx.SwitchState(ctx.IntroState);
			ctx._playIntro = false;
		}
		
		if (ctx._actChange) {
			ctx.SwitchState(ctx.ActChangeState);
			ctx._actChange = false;
		}
		
		if (ctx._gameEnd) {
			ctx.SwitchState(ctx.GameEndState);
		}
		
		
	}
	
}
