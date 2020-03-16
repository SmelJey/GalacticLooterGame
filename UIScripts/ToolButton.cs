using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for Editor Brush-choosers buttons.
/// </summary>
public class ToolButton : MonoBehaviour, IPointerClickHandler {
    public EditorManager editorManager;
    public Level.MapObject brushAlias;

    public void OnPointerClick(PointerEventData eventData) {
        GameLogger.LogMessage($"Brush click is {this.brushAlias}", "ToolButton");
        this.editorManager.SelectBrush(this.brushAlias);
    }
}
