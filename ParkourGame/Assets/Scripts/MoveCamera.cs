using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    public Transform cameraPosition;

    // Update camera position
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
