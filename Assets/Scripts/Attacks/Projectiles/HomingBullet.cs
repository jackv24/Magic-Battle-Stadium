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
        //Get list of potential targets (all players)
        potentialTargets = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        //Remove the owner of this bullet from the list of targets
        potentialTargets.Remove(bullet.owner);

        //Abitrary start distance (all targets should be closer than this)
        float minDistance = 1000f;

        //Iterate through all targets
        foreach (GameObject t in potentialTargets)
        {
            //Find distance to this target
            float distance = Vector3.Distance(transform.position, t.transform.position);

            //If this target is closer than the last, and the target is alive
            if (distance < minDistance && t != bullet.owner && t.GetComponent<PlayerStats>().isAlive)
            {
                //Update min distance
                minDistance = distance;

                //Set this target as the target
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
