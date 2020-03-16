using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates upgrade shop's panel according to <see cref="UpgradeManager"/>.
/// </summary>
public class UpgradeShop : MonoBehaviour {
    public Image shopPanel;
    public GameObject[] buttons;

    public void CloseShop() {
        GameManager.instance.gameState = GameManager.GameState.PLAYING;
        Time.timeScale = 1;
    }

    private void SetupShop() {
        GameLogger.LogMessage("Shop setup", "UpgradeShop");

        var upgradesIt = GameManager.instance.upgradeManager.availableUpgrades.GetEnumerator();
        for (int i = 0; i < this.buttons.Length; i++) {
            if (!upgradesIt.MoveNext()) {
                this.buttons[i].gameObject.SetActive(false);
                continue;
            }

            this.buttons[i].gameObject.SetActive(true);
            var button = this.buttons[i].GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            string key = upgradesIt.Current.Key;

            button.onClick.AddListener(() => {
                GameManager.instance.upgradeManager.TryToBuy(key);
                this.SetupShop();
            });

            GameLogger.LogMessage($"Item in UpgradeShop on {i} position is {key}", "UpgradeShop");

            this.buttons[i].GetComponentInChildren<Text>(true).text = $"{upgradesIt.Current.Value.Name}\r\nCost: {upgradesIt.Current.Value.Cost}";
        }
    }

    private void Start() {
        this.SetupShop();
    }

    private void Update() {
        if (GameManager.instance != null) {
            this.shopPanel.gameObject.SetActive(GameManager.instance.gameState == GameManager.GameState.SHOPPING);
        }
    }
}
