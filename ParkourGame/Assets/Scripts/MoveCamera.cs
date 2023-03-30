using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    private Transform cameraPosition;

    private void Awake()
    {
        cameraPosition = GameObject.FindWithTag("CameraPos").transform;
    }

    // Update camera position
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
