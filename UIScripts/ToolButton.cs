using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for Editor Brush-choosers buttons.
/// </summary>
public class ToolButton : MonoBehaviour, IPointerClickHandler {
    public EditorManager editorManager;
    public Level.MapObject brushAlias;

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log($"Brush click is {this.brushAlias}");
        this.editorManager.SelectBrush(this.brushAlias);
    }
}
