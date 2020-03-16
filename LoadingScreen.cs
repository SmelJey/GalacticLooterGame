using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameState = GameManager.GameState;

/// <summary>
/// Loading screen class.
/// </summary>
public class LoadingScreen : MonoBehaviour {
    public Slider progressBar;
    public Text levelStartButton;

    private static Player playerStorage = null;

    private static bool needPlayerCopy = false;

    public static void StartLoading() {
        GameLogger.LogMessage("Start loading screen", "LoadingScreen");
        Time.timeScale = 0;
        LoadingScreen.needPlayerCopy = false;

        if (GameManager.instance != null) {
            LoadingScreen.needPlayerCopy = true;
            LoadingScreen.playerStorage = GameManager.instance.playerInstance;
            GameManager.instance.gameState = GameState.NOT_LOADED;
        } else {
            LoadingScreen.playerStorage = null;
        }

        GameLogger.LogMessage($"Save player data: {LoadingScreen.needPlayerCopy}", "LoadingScreen");

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

        GameLogger.LogMessage("Reload started", "LoadingScreen");
    }

    private IEnumerator Start() {
        GameLogger.LogMessage("Loading Screen Startup", "LoadingScreen");
        yield return null;
        AsyncOperation loading = SceneManager.LoadSceneAsync(2);

        loading.completed += (asyncOperation) => {
            GameLogger.LogMessage($"Reload of level {GameManager.instance.currentLevelNumber} complete", "LoadingScreen");
            GameManager.instance.Awake();

            if (LoadingScreen.needPlayerCopy) {
                GameManager.instance.playerInstance.CloneStats(LoadingScreen.playerStorage);
            }
        };

        while (!loading.isDone) {
            if (loading.progress < 0.9f) {
                float progress = Mathf.Clamp01(loading.progress / 0.9f);
                this.progressBar.value = progress;
            }

            this.progressBar.value = loading.progress;

            yield return null;
        }
    }
}
