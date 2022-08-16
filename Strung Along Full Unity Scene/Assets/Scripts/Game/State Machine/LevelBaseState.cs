using UnityEngine;

public abstract class LevelBaseState
{
	public abstract void EnterState(GameStateManager ctx);
	
	public abstract void UpdateState(GameStateManager ctx);
}
