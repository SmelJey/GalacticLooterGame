using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Object that can spawn <see cref="spawnling"/>.
/// </summary>
public class Spawner : MonoBehaviour {
    public GameObject spawnling;

    [Header("Spawner attributes")]
    public float spawnRate;
    public float spawnAcceleration;
    public float spawnDelay;

    private float spawnRange = 16f;

    private IEnumerator Spawn(float cooldown) {
        while (true) {
            bool isAbleToSpawn = true;
            foreach (Transform child in this.transform) {
                if (child.position == this.transform.position) {
                    isAbleToSpawn = false;
                    break;
                }
            }

            if (isAbleToSpawn && Random.value < this.spawnRate
                && (GameManager.instance.playerInstance.transform.position - this.transform.position).magnitude > this.spawnRange) {
                GameObject spawnlingInstance = Instantiate(this.spawnling, new Vector3(this.transform.position.x, this.transform.position.y, 0f), Quaternion.identity);
                spawnlingInstance.transform.SetParent(this.transform);
            }

            this.spawnRate *= this.spawnAcceleration;

            yield return new WaitForSeconds(cooldown);
        }
    }

    private void Start() {
        this.GetComponent<SpriteRenderer>().enabled = false;
        this.spawnAcceleration += 0.00025f * GameManager.instance.currentLevelNumber;
        this.StartCoroutine(this.Spawn(this.spawnDelay));
    }
}
