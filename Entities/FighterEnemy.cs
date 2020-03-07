using System.Collections;
using UnityEngine;

/// <summary>
/// Shooting enemy class.
/// </summary>
public sealed class FighterEnemy : Enemy {
    public GameObject firepoint;
    public GameObject bullet;

    [Header("Fighter attributes:")]
    public float shootingRange;
    public float shootingCooldown;
    public int bulletSpeed;
    public int bulletDmg;

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (GameManager.instance.playerInstance == null) {
            return;
        }

        if (Vector2.Distance(this.transform.position, GameManager.instance.playerInstance.transform.position) <= this.shootingRange) {
            RaycastHit2D hit = Physics2D.Linecast(this.transform.position, GameManager.instance.playerInstance.transform.position, LayerMask.GetMask("Walls", "Player"));
            if (hit.collider.CompareTag("Player")) {
                Utility.Rotate(this.gameObject, GameManager.instance.playerInstance.transform.position);

                if (this.currentAction == null) {
                    this.currentAction = this.StartCoroutine(this.Shoot(this.shootingCooldown));
                }

                this.rigidBody.velocity = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// Coroutine for shooting with ShotCD.
    /// </summary>
    private IEnumerator Shoot(float cooldown) {
        Bullet.Instantiate(this.bullet, this.firepoint.transform, this.gameObject, this.bulletDmg, this.bulletSpeed);
        yield return new WaitForSeconds(cooldown);
        this.currentAction = null;
    }
}
