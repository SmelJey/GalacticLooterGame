using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that implements paginated UIElement.
/// </summary>
public class TabPanel : MonoBehaviour {
    public static Color activeColor = new Color(1f, 1f, 1f);
    public static Color normalColor = new Color(0.659f, 0.659f, 0.659f);
    public static Color hoverColor = new Color(0.357f, 0.357f, 0.294f);

    public List<GameObject> pages;
    public List<TabButton> tabs;
    public TabButton selectedTab;

    public void AddTab(TabButton button) {
        if (this.tabs == null) {
            this.tabs = new List<TabButton>();
        }

        this.tabs.Add(button);
    }

    public void OnTabSelect(TabButton tab) {
        this.selectedTab = tab;
        this.ResetTabs();
        tab.sprite.color = TabPanel.activeColor;

        int index = tab.transform.GetSiblingIndex();
        for (int i = 0; i < this.pages.Count; i++) {
            this.pages[i].SetActive(i == index);
        }
    }

    public void OnTabEnter(TabButton tab) {
        this.ResetTabs();
        if (this.selectedTab != null && tab != this.selectedTab) {
            tab.sprite.color = TabPanel.hoverColor;
        }
    }

    public void OnTabExit(TabButton tab) {
        this.ResetTabs();
    }

    private void Start() {
        for (int i = 0; i < this.pages.Count; i++) {
            this.pages[i].SetActive(false);
        }
    }

    /// <summary>
    /// Resets all tabs to default state (excluding <see cref="selectedTab"/>).
    /// </summary>
    private void ResetTabs() {
        foreach (var item in this.tabs) {
            if (this.selectedTab != null && item == this.selectedTab) {
                continue;
            }

            item.sprite.color = TabPanel.normalColor;
        }
    }
}
