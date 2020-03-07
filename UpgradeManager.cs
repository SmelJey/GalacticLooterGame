using System;
using System.Collections.Generic;

/// <summary>
/// Storing and processing upgrades.
/// </summary>
public class UpgradeManager {
    public Dictionary<string, Upgrade> availableUpgrades;

    private List<Upgrade> upgradeTree;

    public void InitUpgradeTree() {
        this.upgradeTree = new List<Upgrade>();
        this.availableUpgrades = new Dictionary<string, Upgrade>();

        this.GenerateBranch(3, "Damage Up", "Damage +10", 300, 100, (Player player) => { player.bulletDmg += 10; });
        this.GenerateBranch(3, "Health Up", "Max HP +10", 400, 75, (Player player) => {
            player.maxHp += 10;
            player.hp += 10;
        });
        this.GenerateBranch(3, "Shield Up", "Max Shield +5", 250, 75, (Player player) => { player.maxShield += 5; });
        this.GenerateBranch(2, "Speed Up", "Max Speed +1", 250, 100, (Player player) => { player.maxSpeed += 1; });
        this.GenerateBranch(2, "Projectile Speed Up", "Bullet's speed +2", 250, 50, (Player player) => { player.bulletSpeed += 2; });
        this.GenerateBranch(2, "Stability Up", "Heating rate -0.01", 350, 100, (Player player) => { player.heatingCdInc -= 0.01f; });
        this.GenerateBranch(1, "Dash Up", "Lower dash's cost by 10", 350, 0, (Player player) => { player.jumpCost -= 10; });

        this.AddAvailableUpgrades();
    }

    public void ResetUpgrades() {
        foreach (var up in this.upgradeTree) {
            up.IsObtained = false;
        }

        this.AddAvailableUpgrades();
    }

    public void TryToBuy(string id) {
        bool result = this.availableUpgrades[id].Buy(GameManager.instance.playerInstance);

        if (result) {
            this.availableUpgrades.Remove(id);
            this.AddAvailableUpgrades();
        }
    }

    private void AddAvailableUpgrades() {
        foreach (var up in this.upgradeTree) {
            if (up.CheckAvailability() && !this.availableUpgrades.ContainsKey(up.Name)) {
                this.availableUpgrades.Add(up.Name, up);
            }
        }
    }

    private Upgrade GenerateBranch(int length, string name, string description, int initialCost, int costStep, Action<Player> callback) {
        int cnt = 1;
        int cost = initialCost;
        var baseUpgrade = new Upgrade($"{name} {cnt}", description, cost, callback);
        var cur = baseUpgrade;
        Upgrade prev = baseUpgrade;
        cost += costStep;
        cnt++;
        this.upgradeTree.Add(cur);

        for (; cnt <= length; cnt++, cost += costStep) {
            cur = new Upgrade(name + cnt, description, cost, callback);
            cur.AddDependency(prev);
            this.upgradeTree.Add(cur);
            prev = cur;
        }

        return baseUpgrade;
    }
}
