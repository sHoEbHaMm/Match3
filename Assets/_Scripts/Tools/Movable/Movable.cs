using System.Collections;
using UnityEngine;

/*
 * This script will allow any game object to be moved smoothly
 * from its current position to a new target position at a speed
 * set in the inspector using a coroutine.
 * 
 * The speed must be a positive number.
 * 
 * You can see if the object is currently moving using Idle
 * 
 * There is an Easing function to alter the speed of the animation over time
 */
public class Movable : MonoBehaviour
{
    private Vector3 from,
                    to;

    private float howfar;

    protected bool idle = true;

    public bool Idle
    {
        get
        {
            return idle;
        }
    }

    [SerializeField]
    private float speed = 1;

    public float Speed
    {
        get
        {
            return speed;
        }
    }

    // coroutine move from current position to new position
    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (speed <= 0)
            Debug.LogWarning("Speed must be a positive number.");

        from = transform.position;
        to = targetPosition;
        howfar = 0;
        idle = false;

        do
        {
            howfar += speed * Time.deltaTime;

            if (howfar > 1)
                howfar = 1;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));

            yield return null;
        }
        while (howfar != 1);

        idle = true;
    }
    // coroutine move from current position to transform, chasing it if it is moving
    public IEnumerator MoveToTransform(Transform target)
    {
        if (speed <= 0)
            Debug.LogWarning("Speed must be a positive number.");

        from = transform.position;
        to = target.position;
        howfar = 0;
        idle = false;

        do
        {
            howfar += speed * Time.deltaTime;

            if (howfar > 1)
                howfar = 1;

            to = target.position;
            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));

            yield return null;
        }
        while (howfar != 1);

        idle = true;
    }
    
    private float Easing(float t)
    {
        return t * t;
    }
}
