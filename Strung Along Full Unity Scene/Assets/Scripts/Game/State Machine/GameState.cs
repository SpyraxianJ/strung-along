using UnityEngine;

public abstract class GameState
{
	public abstract void EnterState(GameStateManager ctx);
	
	public abstract void UpdateState(GameStateManager ctx);
}
