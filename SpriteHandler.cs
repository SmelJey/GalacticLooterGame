using UnityEngine;

/// <summary>
/// ScriptableObject that contains walls' sprites.
/// </summary>
[CreateAssetMenu(fileName = "NewSpriteHandler", menuName = "Walls")]
public class SpriteHandler : ScriptableObject {
    public Sprite[] spriteI;

    public Sprite[] spriteDiagonal;
    public Sprite[] spriteInnerCorner;
    public Sprite[] spriteOuterCorner;
    public Sprite[] spriteSide;

    public Sprite spriteDestruction1Diagonal;
    public Sprite spriteDestruction1InnerCorner;
    public Sprite spriteDestruction1OuterCorner;
    public Sprite spriteDestruction1Side;

    public Sprite spriteDestruction2Diagonal;
    public Sprite spriteDestruction2InnerCorner;
    public Sprite spriteDestruction2OuterCorner;
    public Sprite spriteDestruction2Side;

    public Sprite spriteDestruction3Diagonal;
    public Sprite spriteDestruction3InnerCorner;
    public Sprite spriteDestruction3OuterCorner;
    public Sprite spriteDestruction3Side;

    private Sprite[,] destructionSprites = null;

    public void SetupSprites(Wall wall) {
        wall.AddSprites((char)1, this.spriteOuterCorner);
        wall.AddSprites((char)2, this.spriteOuterCorner);
        wall.AddSprites((char)3, this.spriteSide);
        wall.AddSprites((char)4, this.spriteOuterCorner);
        wall.AddSprites((char)5, this.spriteDiagonal);
        wall.AddSprites((char)6, this.spriteSide);
        wall.AddSprites((char)7, this.spriteInnerCorner);
        wall.AddSprites((char)8, this.spriteOuterCorner);
        wall.AddSprites((char)9, this.spriteSide);
        wall.AddSprites((char)10, this.spriteDiagonal);
        wall.AddSprites((char)11, this.spriteInnerCorner);
        wall.AddSprites((char)12, this.spriteSide);
        wall.AddSprites((char)13, this.spriteInnerCorner);
        wall.AddSprites((char)14, this.spriteInnerCorner);
        wall.AddSprites((char)15, this.spriteI);

        wall.AddRotation(0, 0);
        wall.AddRotation(1, 0);
        wall.AddRotation(2, 270);
        wall.AddRotation(3, 270);
        wall.AddRotation(4, 180);
        wall.AddRotation(5, 270);
        wall.AddRotation(6, 180);
        wall.AddRotation(7, 270);
        wall.AddRotation(8, 90);
        wall.AddRotation(9, 0);
        wall.AddRotation(10, 0);
        wall.AddRotation(11, 0);
        wall.AddRotation(12, 90);
        wall.AddRotation(13, 90);
        wall.AddRotation(14, 180);
        wall.AddRotation(15, 0);
    }

    public Sprite GetDestructionSprites(Wall wall) {
        if (this.destructionSprites == null) {
            this.destructionSprites = new Sprite[4, 3] {
                { this.spriteDestruction1Diagonal, this.spriteDestruction2Diagonal, this.spriteDestruction3Diagonal },
                { this.spriteDestruction1OuterCorner, this.spriteDestruction2OuterCorner, this.spriteDestruction3OuterCorner },
                { this.spriteDestruction1Side, this.spriteDestruction2Side, this.spriteDestruction3Side },
                { this.spriteDestruction1InnerCorner, this.spriteDestruction2InnerCorner, this.spriteDestruction3InnerCorner }
            };
        }

        if (wall.wallType > 3) {
            return null;
        }

        if (wall.hp < wall.maxHp / 4) {
            return this.destructionSprites[wall.wallType, 2];
        }

        if (wall.hp < 2 * wall.maxHp / 4) {
            return this.destructionSprites[wall.wallType, 1];
        }

        if (wall.hp < 3 * wall.maxHp / 4) {
            return this.destructionSprites[wall.wallType, 0];
        }

        return null;
    }
}
