using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;


public class UITransitionManager : MonoBehaviour {
    public CinemachineCamera currentCamera;
    public void Start() {
        currentCamera.Priority++;
    }

    public void UpdateCamera(CinemachineCamera target) {
        currentCamera.Priority--;
        currentCamera = target;
        currentCamera.Priority++;
    }
}