
public class Objective {
    public enum ObjectiveType {
        Empty,
        KillTheBoss,
        MineTheGold,
        KillTheEnemies,
        FindTheKeys,
    }

    public string description;
    public ObjectiveType obj;

    public int oreToMine;
    public int enemiesToKill;
    public int keysToCollect;

    private int enemiesKilled = 0;
    private int oreMined = 0;
    private int keysCollected = 0;

    public bool Check() {
        if (oreToMine > oreMined) {
            return false;
        }

        if (enemiesToKill > enemiesKilled) {
            return false;
        }

        if (keysToCollect > keysCollected) {
            return false;
        }

        if (obj == ObjectiveType.KillTheBoss && GameManager.instance.boss != null && GameManager.instance.boss.hp > 0) {
            return false;
        }

        return true;
    }
}
    
