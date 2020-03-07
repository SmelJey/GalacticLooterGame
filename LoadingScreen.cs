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

    private static int curPlayerMoney = -1;
    private static int curPlayerHp = -1;

    public static void StartLoading() {
        Time.timeScale = 0;

        if (GameManager.instance != null) {
            curPlayerMoney = GameManager.instance.playerInstance.money;
            curPlayerHp = GameManager.instance.playerInstance.hp;
            GameManager.instance.gameState = GameState.NOT_LOADED;
        } else {
            curPlayerHp = -1;
            curPlayerMoney = 0;
        }

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

        Debug.Log("Reload started");
    }

    private IEnumerator Start() {
        yield return null;
        AsyncOperation loading = SceneManager.LoadSceneAsync(2);

        loading.completed += (asyncOperation) => {
            Debug.Log($"Reload complete {GameManager.instance.currentLevelNumber}");
            GameManager.instance.Awake();
            GameManager.instance.playerInstance.money = curPlayerMoney;
            GameManager.instance.playerInstance.hp = curPlayerHp;
        };

        while (!loading.isDone) {
            Debug.Log(loading.progress);
            if (loading.progress < 0.9f) {
                float progress = Mathf.Clamp01(loading.progress / 0.9f);
                this.progressBar.value = progress;
            }

            this.progressBar.value = loading.progress;

            yield return null;
        }
    }
}
