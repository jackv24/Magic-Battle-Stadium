/*
**  CameraFollow.cs: Makes the camera follow a target on the x and y axis
*/

using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    //The target to follow
    public Transform target;

    //The speed at which to follow
    [Range(0, 1f)]
    public float speed = 0.5f;
    //The target position (for lerping)
    private Vector3 targetPos;

    void LateUpdate()
    {
        //Make sure there is a target to follow
        if (target)
        {
            //Only follow on x and y
            targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);

            //Lerp position
            transform.position = Vector3.Lerp(transform.position, targetPos, speed);
        }
    }
}
