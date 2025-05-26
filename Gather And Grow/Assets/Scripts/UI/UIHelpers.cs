using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public static class UIHelpers {
    /// <summary>
    /// Returns true if the current mouse/touch is over any UI element
    /// that sits on the "UI" layer (or whatever layer(s) you designate for your Canvas).
    /// </summary>
    public static bool IsPointerOverUI(out List<GameObject> uiHits) {
        uiHits = new List<GameObject>();

        // 1) Build a pointer event at the current pointer position
        var pe = new PointerEventData(EventSystem.current) {
            position = Mouse.current.position.ReadValue()
        };

        // 2) Raycast against everything the EventSystem knows about
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pe, results);

        // 3) Filter only those on your UI layers
        //    (e.g. you might put all your UI canvases on a layer named "UI")
        int uiLayer = LayerMask.NameToLayer("UI");
        foreach (var r in results) {
            if (r.gameObject.layer == uiLayer)
                uiHits.Add(r.gameObject);
        }

        return uiHits.Count > 0;
    }
}
