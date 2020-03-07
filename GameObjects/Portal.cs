using System.Collections.Generic;
using UnityEngine;
using MapObject = Level.MapObject;

/// <summary>
/// Portal object that can transfer Entities and Bullets.
/// </summary>
public sealed class Portal : MonoBehaviour {
    public static readonly Dictionary<MapObject, Color> Colors = new Dictionary<MapObject, Color>() {
        { MapObject.PortalBlue, new Color(0.48f, 0.48f, 0.8f) },
        { MapObject.PortalGreen, new Color(0.42f, 0.9f, 0.4f) },
        { MapObject.PortalRed, new Color(0.9f, 0.3f, 0.3f) },
        { MapObject.PortalYellow, new Color(0.85f, 0.85f, 0.5f) },
        { MapObject.PortalCyan, new Color(0.45f, 0.9f, 0.9f) },
        { MapObject.PortalMagenta, new Color(0.8f, 0.5f, 0.8f) }
    };

    /// <summary>
    /// Portal exit.
    /// </summary>
    private GameObject portalOut;

    public void SetupPortal(GameObject outObject, MapObject color) {
        this.portalOut = outObject;
        this.GetComponent<SpriteRenderer>().color = Colors[color];

        if (this.portalOut != null) {
            var portalComp = this.portalOut.GetComponent<Portal>();
            if (portalComp != null) {
                portalComp.GetComponent<SpriteRenderer>().color = Colors[color];
                portalComp.portalOut = this.gameObject;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        var entityComp = col.GetComponent<Entity>();
        if (entityComp != null) {
            entityComp.Teleport(this.portalOut);
        }
    }
}
