using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialListener : MonoBehaviour
{
	public enum Tutorials {
		Move,
		Jump,
		Grab,
		Slingshot,
		HoldString,
		Tangle
	}
	
	public GameObject _popup;
	TootyController _tooty;
	Tutorial _currentTutorial = null;
	Tutorials _currentTutorialType;
	GameStateManager _ctx;
	Animator _popupAnimator;
	PuppetController _p1;
	PuppetController _p2;
	bool _condition1 = false;
	bool _condition2 = false;
	bool _exit = false;
	[Space]
	public Image _moveTut;
	public Image _jumpTut;
    public Image _grabTut;
	public Image _slingTut;
	public Image _hangingTut;
	public Image _tangleTut;
    
	
	
    // Start is called before the first frame update
    void Start()
    {
		_tooty = GetComponent<TootyController>();
		_ctx = FindObjectOfType<GameStateManager>();
		_popupAnimator = _popup.GetComponent<Animator>();
		_p1 = _ctx._player1.GetComponent<PuppetController>();
		_p2 = _ctx._player2.GetComponent<PuppetController>();
		
		_popup.SetActive(false);
		
		_currentTutorialType = Tutorials.Move;
		_moveTut.color = new Color(1, 1, 1, 1);
		
		_jumpTut.color = new Color(1, 1, 1, 0);
		_grabTut.color = new Color(1, 1, 1, 0);
		//_slingTut.color = new Color(1, 1, 1, 0);
		//_hangingTut.color = new Color(1, 1, 1, 0);
		//_tangleTut.color = new Color(1, 1, 1, 0);
    }

    void FixedUpdate()
    {
        if (_currentTutorial) CheckExitCondition(_currentTutorialType);
    }
	
	public void PlayTutorial(Tutorial tut) {
		
		 if (tut && tut._tuts.Length > 0) {
			 _currentTutorial = tut;
			StartCoroutine( TutorialRoutine() );
		 }
		
		
	}
	
	IEnumerator TutorialRoutine() {
		yield return new WaitForSeconds(_currentTutorial._initialDelay);
		
		_tooty.RunToPosition( _currentTutorial._ratPosition );
		
		yield return new WaitUntil( () => _tooty._inPlace );
		
		foreach (Tutorials tutType in _currentTutorial._tuts) {
			_condition1 = false;
			_condition2 = false;
			_exit = false;
			
			_currentTutorialType = SetTutorialType(tutType);
			Popup();
			
			while (_exit == false) {
				// wait for players to meet the conditions of this tutorial
				yield return null;
			}
			
			yield return new WaitForSeconds(2.0f);
			Popdown();
			yield return new WaitUntil( () => _popupAnimator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Closed") );
		}
		
		_currentTutorial = null;
		
	}
	
	public void EndTutorial() {
		if (_currentTutorial) {
			Popdown();
			StopAllCoroutines();
			_tooty.RunHome();
		}
		
	}
	
	public void ResetTutorial() {
		if (_currentTutorial) {
			Popdown();
			_tooty.Facepalm();
		}
	}
	
	void Popup() {
		_popup.SetActive(true);
		_popupAnimator.SetBool("open", true);
	}
	
	void Popdown() {
		_popupAnimator.SetBool("open", false);
	}
	
	Tutorials SetTutorialType(Tutorials tutType) {
		GetTutorialImage(_currentTutorialType).color = new Color(1, 1, 1, 0);
		GetTutorialImage(tutType).color = new Color(1, 1, 1, 1);
		return tutType;
	}
	
	Image GetTutorialImage(Tutorials tutType) {
		
		switch (tutType) {
			case Tutorials.Move:
				return _moveTut;
			case Tutorials.Jump:
				return _jumpTut;
			case Tutorials.Grab:
				return _grabTut;
			case Tutorials.Slingshot:
				return _slingTut;
			case Tutorials.HoldString:
				return _hangingTut;
			case Tutorials.Tangle:
				return _tangleTut;
			default:
				return null;
		}
		
	}
	
	void CheckExitCondition(Tutorials tutType) {
		// every tutorial has a different exit condition.
		
		switch (tutType) {
			case Tutorials.Move:
				_condition1 = _p1.movePressed || _condition1;
				_condition2 = _p2.movePressed || _condition2;
				break;
			case Tutorials.Jump:
				_condition1 = _p1.jumpPressed || _condition1;
				_condition2 = _p2.jumpPressed || _condition2;
				break;
			case Tutorials.Grab:
				//_condition1 = both puppets have successfully grabbed an object at least once
				break;
			case Tutorials.Slingshot:
				//_condition1 = p1 has grabbed or been grabbed by p2
				//_condition2 = inverse of that lol
				break;
			case Tutorials.HoldString:
				//_condition1 = p1 has grabbed their string while airborne
				//_condition2 = ayeyadbshjs
				break;
			case Tutorials.Tangle:
				//_condition1 = 
				break;
			
		}
		
		_exit = _condition1 && _condition2;
	}
	
}
