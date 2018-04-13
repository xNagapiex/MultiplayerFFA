using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 13.4.2018 Taru Konttinen
// Camera either follows a given target or stays stationary (default)

public class CameraFollow : MonoBehaviour
{
    Transform Target;

    void Start()
    {
        Target = transform;
    }

    void Update()
    {
        if (Target != transform)
        {
            Vector3 targetPos = Target.position;
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        }
    }

    // Used to set player as the target when the controllable player spawns
    public void SetCameraTarget(Transform newTarget)
    {
        Target = newTarget;
    }
}