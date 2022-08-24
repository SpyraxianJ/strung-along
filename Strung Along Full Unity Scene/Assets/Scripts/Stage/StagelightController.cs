using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagelightController : MonoBehaviour
{
	public Light[] _mains;
	public Light[] _front;
	public Light _player1;
	public Light _player2;
	
	Coroutine _mainsRoutine;
	Coroutine _frontRoutine;
	
	float _mainIntensity;
	float _frontIntensity;
	float _puppetIntensity;
	
    void Start() {
		// keep track of the initial intensity values
        _mainIntensity = _mains[0].intensity;
		_frontIntensity = _front[0].intensity;
		_puppetIntensity = _player1.intensity;
		
		_mainsRoutine = StartCoroutine( MainsFlicker() );
		_frontRoutine = StartCoroutine( FrontFlicker() );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void DimMains() {
		StopCoroutine(_mainsRoutine);
		_mainsRoutine = StartCoroutine( FadeMainsTo(0f) );
	}
	
	public void BrightenMains() {
		StopCoroutine(_mainsRoutine);
		_mainsRoutine = StartCoroutine( FadeMainsTo(_mainIntensity) );
	}
	
	IEnumerator MainsFlicker() {
		foreach (Light light in _mains) {
			if (Random.Range(0, 200) == 69  ) {
				light.intensity = _mainIntensity * 0.5f;
			} else {
				light.intensity = _mainIntensity;
			}
		}
		
		yield return null;
		_mainsRoutine = StartCoroutine( MainsFlicker() );
	}
	
	IEnumerator FadeMainsTo(float targetIntensity) {
		foreach (Light light in _mains) {
			light.intensity = Mathf.MoveTowards(light.intensity, targetIntensity, Time.deltaTime);
			yield return null;
			
			if (light.intensity != targetIntensity) {
				_mainsRoutine = StartCoroutine( FadeMainsTo(targetIntensity) );
			}
		}
		
		if (targetIntensity == _mainIntensity) {
			_mainsRoutine = StartCoroutine( MainsFlicker() );
		}
	}
	
	IEnumerator FrontFlicker() {
		foreach (Light light in _front) {
			if (Random.Range(0, 200) == 69  ) {
				light.intensity = 1.0f;
			} else {
				light.intensity = Random.Range(_frontIntensity * 0.8f, _frontIntensity);
			}
		}
		
		yield return null;
		_frontRoutine = StartCoroutine( FrontFlicker() );
	}
	
	IEnumerator FrontPingPong(int iterations) {
		// bounce the light from left to right, then back.
		float speed = 0.2f;
		
		
		if (iterations > 1) {
			yield return FrontPingPong(iterations - 1);
		} else {
			yield return null;
		}
		
	}
	
}
