using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Class that implements TabButton of <see cref="TabPanel"/>.
/// </summary>
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public TabPanel tabPanel;
    public Image sprite;

    public void OnPointerClick(PointerEventData eventData) {
        this.tabPanel.OnTabSelect(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        this.tabPanel.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        this.tabPanel.OnTabExit(this);
    }

    private void Start() {
        this.tabPanel.AddTab(this);
        this.sprite = this.GetComponent<Image>();
        this.sprite.color = TabPanel.normalColor;
    }
}
