using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game class.
/// </summary>
public class GameManager : MonoBehaviour {
    [HideInInspector]
    public static GameManager instance = null;

    public static IMapGenerator customLevel = null;

    public static bool isDeveloperMode = false;

    public LevelManager levelManager;
    public AudioManager audioManager;
    public GameState gameState = GameState.NOT_LOADED;
    public Player playerInstance;
    public BossEnemy boss = null;

    public int currentLevelNumber = 1;

    public UpgradeManager upgradeManager = new UpgradeManager();

    public enum GameState {
        /// <summary> GameManager or Level is not fully loaded yet. </summary>
        NOT_LOADED,

        /// <summary> Shop is launched </summary>
        SHOPPING,

        /// <summary>  Level is loaded, game is started. </summary>
        PLAYING,

        /// <summary> Player in Pause In-Game menu. </summary>
        PAUSED,

        /// <summary> Game is ended, player in GameOver menu </summary>
        GAME_OVER
    }

    public static void TutorialStart() {
        GameLogger.LogMessage("Tutorial start", "Tutorial");
    }

    public static void RestartGame(IMapGenerator generator) {
        GameLogger.LogMessage("Game restart", "GameManager");

        GameManager.customLevel = generator;

        if (GameManager.instance != null) {
            GameManager.instance.gameState = GameState.NOT_LOADED;
            GameManager.instance.currentLevelNumber = 1;
            GameManager.instance.upgradeManager.ResetUpgrades();
            GameLogger.LogMessage("GameManager instantiated", "GameManager");
        }

        LoadingScreen.StartLoading();
    }

    /// <summary>
    /// Cheatcodes for testers. Press F9 in main menu to enable dev mode, then press enter in pause menu and type one of the next codes.
    /// </summary>
    /// <param name="cmd"> Command to process. </param>
    public static void ProcessCommand(string cmd) {
        if (GameManager.instance == null || GameManager.instance.playerInstance == null || !GameManager.isDeveloperMode) {
            return;
        }

        GameLogger.LogMessage($"Player used command {cmd}", "Console");

        switch (cmd) {
            case "/power_overwhelming": {
                GameManager.instance.playerInstance.shield += 10000;
                break;
            }

            case "/my_meditation_is_over": {
                GameManager.instance.playerInstance.money += 10000;
                break;
            }

            case "/im_a_fat_guy": {
                GameManager.instance.playerInstance.maxHp += 10000;
                GameManager.instance.playerInstance.hp = GameManager.instance.playerInstance.maxHp;
                break;
            }

            case "/orbital_cannon": {
                GameManager.instance.playerInstance.bulletDmg += 1000;
                break;
            }

            case "/lighting_in_the_dark": {
                GameManager.instance.playerInstance.GetComponentInChildren<Light2D>().pointLightOuterRadius = 100;
                break;
            }

            default:
                break;
        }
    }

    public void Awake() {
        GameLogger.LogMessage("Awake status: " + this.gameState.ToString(), "GameManager");
        if (this.gameState != GameState.NOT_LOADED) {
            return;
        }

        this.upgradeManager.InitUpgradeTree();

        Time.timeScale = 0;
        this.gameState = GameState.SHOPPING;

        if (this.currentLevelNumber == 1) {
            GameLogger.LogMessage("First level, skipping shopping stage", "GameManager");
            this.gameState = GameState.PLAYING;
            Time.timeScale = 1;
        }

        if (GameManager.instance == null) {
            GameLogger.LogMessage("GameManager singleton instantiated", "GameManager");
            GameManager.instance = this;
        } else if (GameManager.instance != this) {
            GameLogger.LogMessage("GameManager already exists, deleting this", "GameManager");
            MonoBehaviour.Destroy(this.gameObject);
        }

        GameLogger.LogMessage("Current level is " + this.currentLevelNumber, "GameManager");

        MonoBehaviour.DontDestroyOnLoad(this.gameObject);
        this.levelManager = this.GetComponent<LevelManager>();
        this.audioManager = this.GetComponent<AudioManager>();

        this.StartCoroutine(this.StartGame());
        GameLogger.LogMessage($"Awake ended with state {this.gameState.ToString()}", "GameManager");
    }

    public void NextLevel() {
        if (GameManager.customLevel != null) {
            GameManager.customLevel = null;
            Destroy(GameManager.instance.gameObject);
            SceneManager.LoadScene(0);
            return;
        }

        Time.timeScale = 0;

        this.currentLevelNumber++;
        LoadingScreen.StartLoading();
    }

    public void GameOver() {
        GameLogger.LogMessage("Game over", "GameManager");
        this.gameState = GameState.GAME_OVER;

        Time.timeScale = 0;
    }

    private IEnumerator StartGame() {
        GameLogger.LogMessage("StartGame called", "GameManager");
        this.StartCoroutine(this.CheckLoading());
        this.levelManager.SetupScene(this.currentLevelNumber, GameManager.customLevel);
        this.playerInstance = MonoBehaviour.FindObjectOfType<Player>();
        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Check loading for TL.
    /// </summary>
    private IEnumerator CheckLoading() {
        yield return new WaitForSeconds(60);
        if (this.gameState == GameState.NOT_LOADED) {
            GameLogger.LogError("Loading took too much time", "GameManager");
            Application.Quit();
            
        }
    }
}
