/*
**  HomingBullet.cs: Works in conjuction with bullet.cs to home towards a target
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingBullet : MonoBehaviour
{
    //target to rotate towards
    public GameObject target;
    private List<GameObject> potentialTargets;

    public float rotateSpeed = 0.1f;

    public float homingTime = 3.0f;
    private float stopHomingTime;

    private Bullet bullet;

    void Awake()
    {
        bullet = GetComponent<Bullet>();
    }

    void Start()
    {
        potentialTargets = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        potentialTargets.Remove(bullet.owner);

        float minDistance = 1000f;

        foreach (GameObject t in potentialTargets)
        {
            float distance = Vector3.Distance(transform.position, t.transform.position);

            if (distance < minDistance && t != bullet.owner && t.GetComponent<PlayerStats>().isAlive)
            {
                minDistance = distance;

                target = t;
            }
        }

        stopHomingTime = Time.time + homingTime;
    }

    void Update()
    {
        if (target && Time.time < stopHomingTime)
        {
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;



            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotateSpeed);
        }
    }
}
