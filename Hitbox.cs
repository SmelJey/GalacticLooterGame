using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hitbox of melee attack (deprecated and not used).
/// </summary>
public class Hitbox : MonoBehaviour {
    public List<Entity> hitList;

    private BoxCollider2D boxCollider;
    private string checkTag;

    public void Setup(float range, float size, string tag) {
        this.boxCollider = this.GetComponent<BoxCollider2D>();
        this.boxCollider.size = new Vector2(range, size);
        this.checkTag = tag;
    }

    private void Start() {
        this.hitList = new List<Entity>();
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag(this.checkTag) && !col.isTrigger) {
            this.hitList.Add(col.GetComponent<Entity>());
        }
    }
}
