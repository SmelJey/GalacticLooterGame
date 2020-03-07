using System;
using System.Collections.Generic;

/// <summary>
/// Upgrade class.
/// </summary>
public class Upgrade {
    public List<Upgrade> dependencies = new List<Upgrade>();
    private Action<Player> callback;

    public Upgrade() {
        this.Name = "DefaultName";
        this.Description = "DefaultDesc";
        this.Cost = 1;
        this.callback = (Player player) => { };
    }

    public Upgrade(string upName, string upDesc, int upCost, Action<Player> upCb) {
        this.Name = upName;
        this.Description = upDesc;
        this.Cost = upCost;
        this.callback = upCb;
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Cost { get; set; }

    public bool IsObtained { get; set; } = false;

    /// <summary>
    /// Attempt to but upgrade by a player. Returns false if attempt failed.
    /// </summary>
    /// <param name="player">Buyer.</param>
    /// <returns> False if attempt failed, otherwise true.</returns>
    public bool Buy(Player player) {
        if (!this.CheckAvailability()) {
            return false;
        }

        if (player.money >= this.Cost) {
            player.money -= this.Cost;
            this.callback(player);
            this.IsObtained = true;
            return true;
        }

        return false;
    }

    public void SetCallback(Action<Player> func) {
        this.callback = func;
    }

    /// <summary>
    /// Add dependency upgrade, without which this upgrade is not available.
    /// </summary>
    /// <param name="dependency">Dependency upgrade to add.</param>
    public void AddDependency(Upgrade dependency) {
        this.dependencies.Add(dependency);
    }

    public bool CheckAvailability() {
        if (this.IsObtained) {
            return false;
        }

        foreach (var dep in this.dependencies) {
            if (!dep.IsObtained) {
                return false;
            }
        }

        return true;
    }
}
