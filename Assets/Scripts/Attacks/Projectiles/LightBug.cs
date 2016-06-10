/*
**  LightBug.cs: A type of projectile which waits until a player enters it's range before attacking it.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LightBug : Projectile
{
    //Range at which this light bug can start away from the player
    public float minSpawnDistance = 1f;
    public float maxSpawnDistance = 2f;

    //How fast it should move towards it's target
    [Range(0, 1f)]
    public float lerpSpeed = 0.5f;

    public float idleTime = 1.0f;
    private float startTime = 0;

    public GameObject target;
    private Vector3 targetPos;

    [SyncVar(hook ="UpdateInitialPos")]
    public Vector3 initialPos;

    public override void Start()
    {
        base.Start();

        //AFter being spawned the server decides it's initial target position
        if (isServer)
        {
            initialPos = Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0));
            initialPos *= Random.Range(minSpawnDistance, maxSpawnDistance);
        }

        startTime = Time.time + idleTime;
    }

    void Update()
    {
        //if there is a target, make the target position that of the target
        if (target && Time.time > startTime)
            targetPos = target.transform.position;

        //If not in target position, lerp towards it
        if (transform.position != targetPos && initialPos != Vector3.zero)
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed);
    }

    //After server has set initial pos, clients update
    void UpdateInitialPos(Vector3 pos)
    {
        initialPos = pos;
        targetPos = transform.position + initialPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Add all player that enter, except for the owner
        if (other.tag == "Player" && other.gameObject != owner)
            target = other.gameObject;
    }
}
