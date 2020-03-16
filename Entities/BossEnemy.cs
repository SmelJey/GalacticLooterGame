using System.Collections;
using UnityEngine;

/// <summary>
/// Boss for first Cave location with three types of attack
/// TODO: Make more generalized Boss class.
/// </summary>
public sealed class BossEnemy : Enemy {
    public GameObject[] firepoints;
    public GameObject mainFirepoint;

    public GameObject bullet;
    public GameObject chargedBullet;

    public GameObject exitPortal;

    public string bossName;

    public float salvoCD;
    public float salvoChargeCD;

    public float shotCD;

    public float chargedShotCD;
    public float chargingDelay;

    public float salvoChance;
    public float chargedShotChance;

    public int bulletSpeed;
    public int bulletDmg;

    public int chargedShotSpeed;
    public int chargedShotDmg;

    public int salvoShotSpeed;
    public int salvoShotDmg;

    private int phase = 0;

    private bool isSalvoReady = true;
    private bool isChargedShotReady = true;
    private bool isCharging = false;

    public override void GetHit(int dmg, MonoBehaviour hitter) {
        base.GetHit(dmg, hitter);
        if (this.hp < this.maxHp * 0.5) {
            this.StartCoroutine(this.Salvo(1, 0));
            if (this.phase == 1) {
                this.phase = 2;
                this.salvoChargeCD *= 0.5f;
                this.chargedShotCD *= 0.5f;
                this.maxSpeed *= 2;
            }
        }
    }

    protected override void FixedUpdate() {
        if (this.phase == 0) {
            if (Vector2.Distance(GameManager.instance.playerInstance.transform.position, this.transform.position) > this.detectionRange) {
                return;
            }

            this.phase = 1;
            GameManager.instance.boss = this;
        }

        if (this.isCharging) {
            Utility.Rotate(this.gameObject, GameManager.instance.playerInstance.transform.position);
            return;
        }

        base.FixedUpdate();

        if (this.currentAction == null) {
            if (this.isSalvoReady && Random.value < this.salvoChance) {
                if (Vector2.Distance(this.transform.position, GameManager.instance.playerInstance.transform.position) < 8f) {
                    this.StartCoroutine(this.Salvo(5, 0));
                } else {
                    this.StartCoroutine(this.Salvo(3, Vector2.Distance(this.mainFirepoint.transform.position, this.transform.position)));
                }
            } else if (this.isChargedShotReady && Random.value < this.chargedShotChance) {
                this.StartCoroutine(this.ChargedShot());
            } else {
                this.currentAction = this.StartCoroutine(this.Shoot());
            }
        }
    }

    protected override void CheckDeath() {
        if (this.hp <= 0) {
            Instantiate(this.exitPortal, this.transform.position, Quaternion.identity);

            Spawner[] spawners = Object.FindObjectsOfType<Spawner>();
            foreach (var spawner in spawners) {
                spawner.gameObject.SetActive(false);
            }

            MonoBehaviour.Destroy(this.gameObject);
            return;
        }
    }

    /// <summary>
    /// Shot in sector with center in this.position + offset.
    /// </summary>
    private IEnumerator Salvo(int salvoCount, float offset) {
        this.isSalvoReady = false;

        for (int i = 0; i < salvoCount; i++) {
            yield return new WaitForSeconds(this.salvoChargeCD);
            var objTransform = this.gameObject.transform;

            var playerVec = (GameManager.instance.playerInstance.transform.position - this.gameObject.transform.position).normalized;
            var pointOfView = new Vector2(
                objTransform.position.x + offset * playerVec.x,
                objTransform.position.y + offset * playerVec.y);

            foreach (var point in this.firepoints) {
                Utility.Rotate(point, pointOfView);

                Bullet.Instantiate(this.bullet, point.transform, this.gameObject, this.salvoShotDmg, this.salvoShotSpeed);
            }
        }

        yield return new WaitForSeconds(this.salvoCD);
        this.isSalvoReady = true;
    }

    /// <summary>
    /// Single charged shot into player.
    /// </summary>
    private IEnumerator ChargedShot() {
        this.isChargedShotReady = false;
        this.isCharging = true;

        yield return new WaitForSeconds(this.chargingDelay);
        this.isCharging = false;

        Bullet.Instantiate(this.chargedBullet, this.mainFirepoint.transform, this.gameObject, this.chargedShotDmg, this.chargedShotSpeed);

        yield return new WaitForSeconds(this.chargedShotCD);

        this.isChargedShotReady = true;
    }

    /// <summary>
    /// Single shot into player.
    /// </summary>
    private IEnumerator Shoot() {
        int curPoint = Mathf.RoundToInt(Random.value * (this.firepoints.Length / 2 - 1));

        Utility.Rotate(this.firepoints[curPoint * 2], GameManager.instance.playerInstance.transform.position);
        Utility.Rotate(this.firepoints[curPoint * 2 + 1], GameManager.instance.playerInstance.transform.position);

        Bullet.Instantiate(this.bullet, this.firepoints[curPoint * 2].transform, this.gameObject, this.bulletDmg, this.bulletSpeed);
        Bullet.Instantiate(this.bullet, this.firepoints[curPoint * 2 + 1].transform, this.gameObject, this.bulletDmg, this.bulletSpeed);

        yield return new WaitForSeconds(this.shotCD);
        this.currentAction = null;
    }
}
