using UnityEngine;

public class LevelEmptyState : LevelBaseState
{
	public override void EnterState(GameStateManager ctx) {
		// TODO: ragdoll puppets!
		Debug.Log("Stage is clear!");
		ctx._currentLevel = null;
		
		// TODO: level skip!
	}
	
	public override void UpdateState(GameStateManager ctx) {
		// in this state we sort out what level to load next. in most cases
		// just check which level is next in line and load that one!
		ctx._quitting = false;
		
		if (ctx._nextLevelToLoad != null) {
			
			ctx._currentLevel = ctx._nextLevelToLoad;
			ctx._nextLevelToLoad = ctx._database.GetLevelAfter(ctx._currentLevel);
			
			
			
			ctx.SwitchState(ctx.LoadingState);
		}
		
	}
}
