using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Class used to control In-Game UI.
/// </summary>
public class GameMenuController : MonoBehaviour {
    public Text hpDisplay;
    public Text shieldDisplay;
    public Text scoreDisplay;
    public Text fuelDisplay;
    public Text levelDisplay;

    public Text bossDisplay;

    public Text devmodeDisplay;

    public InputField console;

    public Image gameOverMenu;
    public Image pauseMenu;

    public Camera minimapCamera;

    public void RestartGame() {
        Destroy(GameManager.instance.gameObject);
        GameManager.instance = null;
        GameManager.RestartGame(GameManager.customLevel);
    }

    public void ExitGame() {
        Destroy(GameManager.instance.gameObject);
        SceneManager.LoadScene(0);
    }

    public void ContinueGame() {
        GameManager.instance.gameState = GameManager.GameState.PLAYING;
        GameManager.ProcessCommand(this.console.text);
        this.console.gameObject.SetActive(false);
        this.console.text = string.Empty;
        Time.timeScale = 1.0f;
    }

    private void Update() {
        if (GameManager.instance != null && Keyboard.current.enterKey.wasPressedThisFrame && GameManager.instance.gameState == GameManager.GameState.PAUSED) {
            if (this.console.gameObject.activeInHierarchy) {
                GameManager.ProcessCommand(this.console.text);
                this.console.gameObject.SetActive(false);
                this.console.text = string.Empty;
            } else {
                this.console.gameObject.SetActive(true);
                this.console.Select();
                this.console.ActivateInputField();
            }
        }

        if (GameManager.instance != null && GameManager.instance.playerInstance != null) {
            this.minimapCamera.transform.position = new Vector3(GameManager.instance.playerInstance.transform.position.x, GameManager.instance.playerInstance.transform.position.y, -10);

            this.hpDisplay.text = "HP: " + GameManager.instance.playerInstance.hp;
            this.shieldDisplay.text = "Shields: " + GameManager.instance.playerInstance.shield;
            this.scoreDisplay.text = "Score: " + GameManager.instance.playerInstance.money;
            this.fuelDisplay.text = $"Energy: {(int)GameManager.instance.playerInstance.energy}";

            this.levelDisplay.text = $"Level: {GameManager.instance.currentLevelNumber}";

            this.bossDisplay.enabled = GameManager.instance.boss != null;
            if (GameManager.instance.boss != null) {
                this.bossDisplay.text = $"{GameManager.instance.boss.name} {GameManager.instance.boss.hp}";
            }
        }

        this.gameOverMenu.gameObject.SetActive(GameManager.instance == null || GameManager.instance.gameState == GameManager.GameState.GAME_OVER);
        this.pauseMenu.gameObject.SetActive(GameManager.instance != null && GameManager.instance.gameState == GameManager.GameState.PAUSED);
    }

    private void Start() {
        this.devmodeDisplay.gameObject.SetActive(GameManager.isDeveloperMode);
        this.devmodeDisplay.text = $"DEV MODE {Utility.Version}";
        this.console.gameObject.SetActive(false);
    }
}
