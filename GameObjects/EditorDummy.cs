using UnityEngine;

/// <summary>
/// Dummy, which incapsulates GameObject's logic in editor.
/// </summary>
public class EditorDummy : MonoBehaviour {
    public GameObject attachedObject;

    /// <summary>
    /// Set Dummy's sprite the same as on original object.
    /// </summary>
    public void UpdateSprite() {
        var spriteRenderer = this.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = this.attachedObject.GetComponent<SpriteRenderer>().sprite;
        spriteRenderer.color = this.attachedObject.GetComponent<SpriteRenderer>().color;
        spriteRenderer.sortingLayerID = this.attachedObject.GetComponent<SpriteRenderer>().sortingLayerID;
    }
}
