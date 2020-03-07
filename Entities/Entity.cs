using System.Collections;
using UnityEngine;

/// <summary>
/// Basic class for any moveable object.
/// </summary>
public abstract class Entity : MonoBehaviour, IHittable {
    [Header("Entity attributes:")]
    public int maxHp;
    public int hp;

    public int maxShield;
    public int shield;
    public float shieldRenegerationDelay;
    public int shieldRegeneration;

    public float maxSpeed;

    /// <summary> Acceleration force. </summary>
    public float force;

    /// <summary>
    /// Must be nulled in the end of Coroutine.
    /// </summary>
    protected Coroutine currentAction = null;

    protected CircleCollider2D entityCollider;
    protected Rigidbody2D rigidBody;

    protected ParticleSystem particleLauncher;

    private float currentRegenDelay = 0f;

    private float teleportCooldown = 1f;
    private bool canTeleport = true;

    public virtual void Teleport(GameObject destination) {
        if (this.canTeleport) {
            this.StartCoroutine(this.TeleportCooldown());
            this.transform.position = destination.transform.position;
        }
    }

    /// <summary>
    /// TODO: Make object to fly away in direction opposite to hitter if shield is breaking.
    /// </summary>
    /// <param name="dmg"> Dmg value. </param>
    /// <param name="hitter"> Object that hit this. </param>
    public virtual void GetHit(int dmg, MonoBehaviour hitter) {
        if (this.shield > 0) {
            this.shield -= Mathf.Min(dmg, this.shield);
            float knockbackCoef = (float)dmg / (this.maxShield + this.maxHp);
            this.rigidBody.AddForce(hitter.transform.right * knockbackCoef * 100);
        } else {
            this.hp -= dmg;
            this.CheckDeath();
        }

        this.currentRegenDelay = this.shieldRenegerationDelay;
    }

    /// <summary>
    /// Regenerate shield every <paramref name="cooldown"/> seconds.
    /// </summary>
    /// <param name="cooldown"> Cooldown of regenerating. </param>
    protected virtual IEnumerator RegenShields(float cooldown) {
        while (true) {
            if (this.shield < this.maxShield) {
                if (this.currentRegenDelay > 0) {
                    this.currentRegenDelay -= cooldown;
                } else {
                    this.shield += Mathf.Min(this.shieldRegeneration, this.maxShield - this.shield);
                }
            }

            yield return new WaitForSeconds(cooldown);
        }
    }

    protected virtual void Start() {
        this.entityCollider = this.GetComponent<CircleCollider2D>();
        this.rigidBody = this.GetComponent<Rigidbody2D>();
        this.particleLauncher = this.GetComponent<ParticleSystem>();

        if (this.hp == -1) {
            this.hp = this.maxHp;
        }

        if (this.shield == -1) {
            this.shield = this.maxShield;
        }

        if (this.maxShield > 0) {
            this.StartCoroutine(this.RegenShields(0.5f));
        }
    }

    /// <summary>
    /// Move in direction with maxSpeed.
    /// </summary>
    /// <param name="direction"> direction of movement. </param>
    protected virtual void Move(Vector2 direction) {
        this.rigidBody.AddForce(this.force * direction);
        if (this.rigidBody.velocity.magnitude > this.maxSpeed) {
            this.rigidBody.velocity = this.maxSpeed * this.rigidBody.velocity.normalized;
        }
    }

    protected virtual void CheckDeath() {
        if (this.hp <= 0) {
            MonoBehaviour.Destroy(this.gameObject);
            return;
        }
    }

    protected virtual void Update() {
        this.CheckDeath();
        if (!this.enabled) {
            return;
        }

        if (this.particleLauncher != null) {
            ParticleSystem.EmissionModule emmissionModule = this.particleLauncher.emission;
            emmissionModule.rateOverTime = (int)((float)(this.maxHp - this.hp) / this.maxHp * 100);
        }
    }

    private IEnumerator TeleportCooldown() {
        this.canTeleport = false;
        yield return new WaitForSeconds(this.teleportCooldown);
        this.canTeleport = true;
    }
}
