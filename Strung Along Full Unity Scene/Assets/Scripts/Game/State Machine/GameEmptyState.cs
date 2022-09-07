using UnityEngine;

public class GameEmptyState : GameState
{
	public override void EnterState(GameStateManager ctx) {
		Debug.Log("Stage is clear!");
		ctx._currentLevel = null;
		
		CheckFlags(ctx);
	}
	
	public override void UpdateState(GameStateManager ctx) {
		// in this state we sort out what level to load next. in most cases
		// just check which level is next in line and load that one!
		
		if (ctx._nextLevelToLoad != null) {
			ctx._currentLevel = ctx._nextLevelToLoad;
			ctx._nextLevelToLoad = ctx._database.GetLevelAfter(ctx._currentLevel);
			ctx.SwitchState(ctx.LoadingState);
		}
		
	}
	
	void CheckFlags(GameStateManager ctx) {
		
		if (ctx._playIntro) {
			ctx.SwitchState(ctx.IntroState);
			ctx._playIntro = false;
		}
		
	}
	
}
