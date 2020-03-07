using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for self-destructing enemy.
/// </summary>
public sealed class SuicideEnemy : Enemy {
    [Header("Suicider Attributes:")]
    public float chargeRange;
    public float chargeSpeed;

    public int dmg;

    private float defaultSpeed;

    protected override void FixedUpdate() {
        if (Vector2.Distance(this.transform.position, GameManager.instance.playerInstance.transform.position) <= this.chargeRange) {
            RaycastHit2D hit = Physics2D.Linecast(this.transform.position, GameManager.instance.playerInstance.transform.position, LayerMask.GetMask("Walls", "Player"));
            if (hit.collider.CompareTag("Player")) {
                Utility.Rotate(this.gameObject, GameManager.instance.playerInstance.transform.position);

                this.maxSpeed = this.chargeSpeed;
                this.Move(((Vector2)GameManager.instance.playerInstance.transform.position - this.rigidBody.position).normalized);

                return;
            }
        }

        this.maxSpeed = this.defaultSpeed;
        base.FixedUpdate();
    }

    protected override void Start() {
        base.Start();
        this.defaultSpeed = this.maxSpeed;
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.CompareTag("Player")) {
            var playerComp = col.collider.GetComponent<Player>();
            if (playerComp != null) {
                playerComp.GetHit(this.dmg, this);
                MonoBehaviour.Destroy(this.gameObject);
            }
        }
    }
}
