using UnityEngine;
using MapObject = Level.MapObject;

/// <summary>
/// DEPRECATED
/// Class for immovable destructible objects.
/// </summary>
[System.Obsolete("Use Wall class instead", true)]
public class Blockage : MonoBehaviour, IHittable {
    /// <summary>
    /// Sprites for different hp bounds.
    /// </summary>
    public Sprite[] dmgSprites;

    [Header("Blockage attributes:")]
    public int maxHp;
    public int hp;         // -1 == maxHp
    public int goldValue;
    public int hpValue;

    /// <summary>
    /// Hp border on which blockage sprite will be changed.
    /// </summary>
    private int[] hpBorders;
    private SpriteRenderer spriteRenderer;

    public void GetHit(int dmg, MonoBehaviour hitter) {
        this.hp -= dmg;
        if (this.hp <= 0) {
            if (hitter is Player) {
                var player = hitter as Player;
                player.money += this.goldValue;
            } else if (hitter is Bullet) {
                var bullet = hitter as Bullet;
                if (ReferenceEquals(GameManager.instance.playerInstance.gameObject, bullet.shooter)) {
                    GameManager.instance.playerInstance.money += this.goldValue;
                    GameManager.instance.playerInstance.hp += Mathf.Min(this.hpValue, GameManager.instance.playerInstance.maxHp - GameManager.instance.playerInstance.hp);
                }
            }

            GameManager.instance.levelManager.currentLevel
                .objectMap[Mathf.RoundToInt(this.transform.position.y)][Mathf.RoundToInt(this.transform.position.x)] = MapObject.Floor;
            MonoBehaviour.Destroy(this.gameObject);
        } else {
            this.spriteRenderer.sprite = this.dmgSprites[this.GetSprite()];
        }
    }

    /// <summary>
    /// Find sprite matching current hp.
    /// </summary>
    /// <returns> Index of matching sprite. </returns>
    private int GetSprite() {
        int i = 0;
        while (this.hp > this.hpBorders[i]) {
            i++;
        }

        return this.dmgSprites.Length - i;
    }

    private void Awake() {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();

        // +1 to prevent out of bounds
        this.hpBorders = new int[this.dmgSprites.Length + 1];

        // Calculate borders for sprite switching
        int hpBorder = this.maxHp / this.dmgSprites.Length;
        this.hpBorders[0] = -1;
        for (int i = 1; i < this.dmgSprites.Length; i++) {
            this.hpBorders[i] = this.hpBorders[i - 1] + hpBorder;
        }

        this.hpBorders[this.dmgSprites.Length] = this.maxHp + 1;

        if (this.hp == -1) {
            this.hp = this.maxHp;
        } else if (this.hp <= 0) {
            this.gameObject.SetActive(false);
        } else {
            this.spriteRenderer.sprite = this.dmgSprites[this.GetSprite()];
        }
    }
}
