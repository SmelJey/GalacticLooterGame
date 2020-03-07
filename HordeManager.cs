using System.Collections;
using UnityEngine;

/// <summary>
/// Generates horde every <see cref="hordeCooldown"/> seconds.
/// </summary>
public class HordeManager : MonoBehaviour {
    public GameObject[] hordeEntities;
    public int[] entitiesCounts;

    /// <summary> Increase number of enemies on each wave. </summary>
    public int[] dCounts;

    public GameObject hordeDisplay;

    public int hordeCooldown;

    private float minDist = 25f;

    private void Start() {
        if (this.hordeEntities.Length != this.entitiesCounts.Length || this.entitiesCounts.Length != this.dCounts.Length) {
            throw new System.Exception("Arrays have different lengths");
        }

        if (GameManager.instance.currentLevelNumber == 5) {
            return;
        }

        for (int i = 0; i < this.dCounts.Length; i++) {
            this.dCounts[i] += this.dCounts[i] * GameManager.instance.currentLevelNumber / 5;
        }

        this.StartCoroutine(this.Setup(GameManager.instance.levelManager.currentLevel));
    }

    private IEnumerator Setup(Level level) {
        yield return new WaitUntil(() => { return GameManager.instance.gameState == GameManager.GameState.PLAYING; });

        this.StartCoroutine(this.SpawnCycle(level));
    }

    private IEnumerator SpawnCycle(Level level) {
        while (true) {
            yield return new WaitForSeconds(this.hordeCooldown);

            var posToSpawn = level.playerStart;

            while (Vector2.Distance(posToSpawn, GameManager.instance.playerInstance.transform.position) < this.minDist) {
                yield return null;
            }

            for (int i = 0; i < this.hordeEntities.Length; i++) {
                for (int j = 0; j < this.entitiesCounts[i]; j++) {
                    var instance = Instantiate(this.hordeEntities[i], posToSpawn, Quaternion.identity);
                    instance.GetComponent<Enemy>().detectionRange = 1000;
                }

                this.entitiesCounts[i] += this.dCounts[i];
            }

            this.hordeDisplay.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            this.hordeDisplay.SetActive(false);
        }
    }
}
