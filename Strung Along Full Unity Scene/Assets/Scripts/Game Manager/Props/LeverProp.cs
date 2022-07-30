using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
The lever moves a prop to another position when it's triggered.
*/
public class LeverProp : MonoBehaviour
{
	public List<Reaction> _reactions;

	
	[Header("Lever Properties")]
	public bool _player1 = true;
	public bool _player2 = true;
	[Tooltip("Should the lever reset after it's activated?")]
	public bool _resetAfterUse = false;
	// i dunno!
	
	
	
	//public ReactionContext _actvCtx; extra info about who activating etc?
	private GameStateManager _ctx;
	private Transform _leverBase;
	private Transform _leverHandle;
	private float _cooldown = 3.0f;
	
    // Start is called before the first frame update
    void Start()
    {
        _ctx = GetComponentInParent<GameStateManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}