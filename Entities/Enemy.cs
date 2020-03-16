using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class with basic logic for enemies
/// All enemies must inherit this class.
/// </summary>
public abstract class Enemy : Entity {
    [Header("Enemy attributes:")]
    public float detectionRange;
    public float detectionRangeExpanding;

    private readonly float nextWaypointDistance = 1f;

    private List<Vector2Int> path;
    private int currentWaypoint;
    private bool checking = false;

    protected virtual void MoveToPlayer() {
        if (this.path == null || this.currentWaypoint >= this.path.Count) {
            return;
        }

        Utility.Rotate(this.gameObject, this.path[this.currentWaypoint]);
        Vector2 dir = (this.path[this.currentWaypoint] - this.rigidBody.position).normalized;
        this.Move(dir);

        if (!this.checking) {
            StartCoroutine(CheckStuck());
        }
       

#if DEBUG
        for (int i = 0; i < this.path.Count - 1; i++) {
            Debug.DrawLine(
                new Vector3(this.path[i].x, this.path[i].y, 0),
                new Vector3(this.path[i + 1].x, this.path[i + 1].y, 0),
                Color.red);
        }

        Debug.DrawLine(
            this.transform.position,
            new Vector3(this.path[this.currentWaypoint].x, this.path[this.currentWaypoint].y, 0),
            Color.green);
#endif

        if (Vector2.Distance(this.transform.position, this.path[this.currentWaypoint]) < this.nextWaypointDistance) {
            this.currentWaypoint++;
        }
    }

    protected virtual void FixedUpdate() {
        if (Vector2.Distance(GameManager.instance.playerInstance.transform.position, this.transform.position) <= this.detectionRange) {
            this.MoveToPlayer();
        }
    }

    protected IEnumerator UpdatePath(float delayBeforeUpdate) {
        this.path = new List<Vector2Int>();
        while (true) {
            if (GameManager.instance.playerInstance == null) {
                yield return null;
            }

            yield return this.StartCoroutine(GameManager.instance.levelManager
                .currentLevel.Pathfinding(
                new Vector2Int(Mathf.RoundToInt(this.transform.position.x),
                               Mathf.RoundToInt(this.transform.position.y)),
                new Vector2Int(Mathf.RoundToInt(GameManager.instance.playerInstance.transform.position.x),
                               Mathf.RoundToInt(GameManager.instance.playerInstance.transform.position.y)),
                this.path));
            float lastDistance = 10e9f;
            for (this.currentWaypoint = 0; this.currentWaypoint < this.path.Count; this.currentWaypoint++) {
                float newDist = Vector2.Distance(this.transform.position, this.path[this.currentWaypoint]);
                if (newDist > lastDistance) {
                    this.currentWaypoint--;
                    break;
                }

                lastDistance = newDist;
            }

            yield return new WaitForSeconds(delayBeforeUpdate);
        }
    }

    protected override void Start() {
        base.Start();

        this.StartCoroutine(this.UpdatePath(0.5f));
        this.StartCoroutine(this.DetectionRangeExpanding(5f));
    }

    private IEnumerator DetectionRangeExpanding(float delayBeforeExpand) {
        while (true) {
            this.detectionRange += this.detectionRangeExpanding;
            yield return new WaitForSeconds(delayBeforeExpand);
        }
    }

    private IEnumerator CheckStuck() {
        this.checking = true;
        Vector2 startPos = this.transform.position;
        int waypoint = this.currentWaypoint;

        yield return new WaitForSeconds(1);
        float cachedForce = this.force;
        int tries = 0;
        int maxTries = Random.Range(2, 7);

        while (this.path != null && this.currentWaypoint == waypoint && Vector2.Distance(startPos, this.transform.position) < 0.5 && tries < maxTries) {
            startPos = this.transform.position;
            waypoint = this.currentWaypoint;
            this.force = 0;
            tries++;

            yield return new WaitForSeconds(2);
        }

        this.force = cachedForce;
        this.checking = false;
    }
}
