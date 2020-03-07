using UnityEngine;

/// <summary>
/// Simple projectile that can hit IHittable objects and perform some logic on hit.
/// </summary>
public class Bullet : Entity {
    public GameObject impact;

    [Header("Bullet attributes")]
    public int dmg;

    public GameObject shooter;

    /// <summary>
    /// Instantiate <paramref name="bullet"/> with position and rotation of <paramref name="transform"/>
    /// and setting shooter to <paramref name="parent"/>.
    /// </summary>
    /// <param name="bullet"> Bullet to instantiate. </param>
    /// <param name="transform"> Transform which position and rotation should use. </param>
    /// <param name="parent"> Shooter's object. </param>
    /// <param name="damage"> Damage of bullet. </param>
    /// <param name="speed"> Speed of bullet. </param>
    /// <returns> Instantiated object. </returns>
    public static GameObject Instantiate(GameObject bullet, Transform transform, GameObject parent, int damage, int speed) {
        var obj = Instantiate(bullet, transform.position, transform.rotation);
        var bulletComponent = obj.GetComponent<Bullet>();
        bulletComponent.shooter = parent;
        bulletComponent.dmg = damage;
        bulletComponent.maxSpeed = speed;
        return obj;
    }

    public override void Teleport(GameObject destination) {
        base.Teleport(destination);
        this.shooter = destination;
    }

    protected override void Start() {
        base.Start();
        this.rigidBody.velocity = this.transform.right * this.maxSpeed;
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Bullet") || col.CompareTag("Item") || col.gameObject == this.shooter) {
            return;
        }

        var hitted = col.gameObject.GetComponent<IHittable>();
        if (hitted != null) {
            hitted.GetHit(this.dmg, this);
        }

        Instantiate(this.impact, this.transform.position, this.transform.rotation);
        MonoBehaviour.Destroy(this.gameObject);
    }
}
