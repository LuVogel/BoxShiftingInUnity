using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private bool loadIsPressed = false;

    private void OnMouseDown() {
        loadIsPressed = true;
    }

    public bool IsLoadPressed() {
        if (loadIsPressed) {
            loadIsPressed = false;
            return true;
        }
        return false;
    }
}
