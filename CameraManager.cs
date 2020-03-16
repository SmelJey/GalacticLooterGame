using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class for controlling Camera movement In-Game.
/// </summary>
public class CameraManager : MonoBehaviour {
    public GameObject gameManager;

    private void Awake() {
        if (GameManager.instance == null) {
            Object.Instantiate(this.gameManager);
        }

        var pos = GameManager.instance.levelManager.currentLevel.playerStart;
        this.transform.position = new Vector3(pos.x, pos.y, -10);
    }

    private void Start() {
        var pos = GameManager.instance.levelManager.currentLevel.playerStart;
        this.transform.position = new Vector3(pos.x, pos.y, -10);
    }

    private void LateUpdate() {
        if (GameManager.instance != null && GameManager.instance.gameState == GameManager.GameState.PLAYING) {
            var mousePos = this.GetComponent<Camera>().ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var playerPos = GameManager.instance.playerInstance.transform.position;
            this.transform.position = new Vector3(playerPos.x + (mousePos.x - playerPos.x) / 8, playerPos.y + (mousePos.y - playerPos.y) / 6, -10);
        }
    }
}
