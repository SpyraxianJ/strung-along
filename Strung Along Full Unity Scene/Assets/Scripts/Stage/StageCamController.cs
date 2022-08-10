using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// switches cameras via script by modifying the Priority values.
// to activate a new camera, it's Priority is set to 10 and the 
// currently active camera's Priority is set to 1.
public class StageCamController : MonoBehaviour
{
	public CinemachineVirtualCamera[] _cameras;
	public int _initialCam = 0;
	
	CinemachineBrain _brain;
	
    void Start()
    {
        _brain = GetComponentInChildren<CinemachineBrain>();
		
		foreach (CinemachineVirtualCamera cam in _cameras) {
			cam.Priority = 1;
		}
		
		_cameras[_initialCam].Priority = 10;
    }
	
	public void SwitchCam(int cam) {
		if (_brain.IsLive(_cameras[cam]) == false) {
			_brain.ActiveVirtualCamera.Priority = 1;
			_cameras[cam].Priority = 10;
		}
	}

}
