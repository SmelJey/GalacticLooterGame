using System.Collections.Generic;
using UnityEngine;
using MapObject = Level.MapObject;

/// <summary>
/// Destructible wall class with dynamic sprite updating.
/// </summary>
public class Wall : MonoBehaviour, IHittable {
    public SpriteHandler spriteHandler;
    public GameObject damageObject;
    public GameObject icon;

    [Header("Wall's attributes")]
    public int maxHp;
    public int hp;

    public int goldValue;
    public int hpValue;

    [Header("Debug only")]
    public int wallType = -1;
    public int code = -1;

    private Dictionary<int, List<Sprite>> sprites;
    private SpriteRenderer spriteRenderer;

    private Dictionary<int, int> rotations;

    private PolygonCollider2D col;

    private List<Vector2[]> colShapes = new List<Vector2[]> {
        new Vector2[] { new Vector2(-0.5f, 0f), new Vector2(-0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0.5f, -0.5f), new Vector2(0f, -0.5f) },
        new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, 0), new Vector2(0, 0.5f) },
        new Vector2[] { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, -0.5f) },
        new Vector2[] { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0f, -0.5f) },
        new Vector2[] { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f) }
    };

    private int x;
    private int y;

    public void GetHit(int dmg, MonoBehaviour hitter) {
        if (this.maxHp < 0) {
            return;
        }

        this.hp -= dmg;
        if (this.hp <= 0) {
            Vector2Int hitpoint = new Vector2Int(this.x + 1, this.y);
            if (Level.Walls.Contains(GameManager.instance.levelManager.currentLevel
                .objectMap[this.y][this.x])) {
                hitpoint = new Vector2Int(this.x, this.y);
            } else if (Level.Walls.Contains(GameManager.instance.levelManager.currentLevel
                .objectMap[this.y + 1][this.x])) {
                hitpoint = new Vector2Int(this.x, this.y + 1);
            } else if (Level.Walls.Contains(GameManager.instance.levelManager.currentLevel
                .objectMap[this.y + 1][this.x + 1])) {
                hitpoint = new Vector2Int(this.x + 1, this.y + 1);
            }

            if (!Level.Walls.Contains(GameManager.instance.levelManager.currentLevel
                .objectMap[hitpoint.y][hitpoint.x])) {
                MonoBehaviour.Destroy(this.gameObject);
                return;
            }

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
                .objectMap[hitpoint.y][hitpoint.x] = MapObject.Floor;

            for (int i = -1; i <= 1; i++) {
                int[] codes = new int[4];
                for (int j = -1; j <= 1; j++) {
                    if (i == 0 && j == 0) {
                        continue;
                    }

                    GameManager.instance.levelManager.currentLevel.UpdateWallState(this.x + j, this.y + i);
                    codes[j + 1] = GameManager.instance.levelManager.currentLevel.wallMap[this.y + i][this.x + j];
                }
            }

            this.hp = this.maxHp;
            GameManager.instance.levelManager.currentLevel.UpdateWallState(this.x, this.y);
        } else {
            this.damageObject.GetComponent<SpriteRenderer>().sprite = this.spriteHandler.GetDestructionSprites(this);
        }
    }

    public GameObject InstantiateWall(int x, int y, Level level) {
        var obj = Instantiate(this.gameObject, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity) as GameObject;
        var wallComp = obj.GetComponent<Wall>();
        wallComp.x = x;
        wallComp.y = y;
        wallComp.UpdateSprite(level);
        return obj;
    }

    public void UpdateSprite(Level level) {
        if (this.code != -1) {
            this.transform.Rotate(new Vector3(0, 0, -this.rotations[this.code]));
        }

        this.code = level.wallMap[this.y][this.x];

        if (this.code == 0) {
            GameManager.instance.levelManager.currentLevel
                .wallObjects[this.y][this.x] = null;
            MonoBehaviour.Destroy(this.gameObject);
            return;
        }

        this.spriteRenderer.sprite = Utility.SelectRandomItem(this.sprites[level.wallMap[this.y][this.x]]);

        int bitnumber = 0;
        if ((this.code & 2) == 2) {
            bitnumber++;
        }

        if ((this.code & 4) == 4) {
            bitnumber++;
        }

        if ((this.code & 8) == 8) {
            bitnumber++;
        }

        if ((this.code & 1) == 1) {
            bitnumber++;
        }

        if (bitnumber == 2) {
            if (this.code == 5 || this.code == 10) {
                bitnumber -= 2;
            }
        }

        if (this.icon != null) {
            this.icon.SetActive(bitnumber != 1);
        }

        this.wallType = bitnumber;
        this.col.SetPath(0, this.colShapes[bitnumber]);
        this.transform.Rotate(new Vector3(0, 0, this.rotations[level.wallMap[this.y][this.x]]));
        if (this.maxHp >= 0) {
            this.damageObject.GetComponent<SpriteRenderer>().sprite = this.spriteHandler.GetDestructionSprites(this);
        }
    }

    public void AddSprites(char mark, Sprite[] spriteArray) {
        this.sprites.Add(mark, new List<Sprite>());
        foreach (var sprite in spriteArray) {
            this.sprites[mark].Add(sprite);
        }
    }

    public void AddRotation(int code, int rotation) {
        this.rotations.Add(code, rotation);
    }

    private void Awake() {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();

        this.sprites = new Dictionary<int, List<Sprite>>();
        this.rotations = new Dictionary<int, int>();
        this.spriteHandler.SetupSprites(this);

        this.col = this.GetComponent<PolygonCollider2D>();
    }
}
