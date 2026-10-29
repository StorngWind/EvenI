using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform Target;
    float OffsetX;

    void Start ()
    {
        if (Target == null)
            return;

        OffsetX = transform.position.x - Target.position.x;
    }
	
    void Update ()
    {
        if (Target == null)
            return;

        Vector3 pos = transform.position;
        pos.x = Target.position.x + OffsetX;
        transform.position = pos;
    }
}
