using UnityEngine;

/// <summary>
/// Interface for objects, that can interact with projectiles.
/// </summary>
public interface IHittable {
    void GetHit(int dmg, MonoBehaviour hitter);
}
