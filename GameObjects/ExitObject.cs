using UnityEngine;

/// <summary>
/// Object which player should touch to complete level.
/// </summary>
public sealed class ExitObject : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Player")) {
            GameManager.instance.NextLevel();
        }
    }
}
