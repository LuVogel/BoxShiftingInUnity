using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResetButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    public Vector2 pos => transform.position;
    [SerializeField] private bool pressed = false;

    public bool ResetIsPressed() {
        if (pressed) {
            pressed = false;
            return true;
        }
        return false;
    }

    private void OnMouseDown() {
        pressed = true;
    }
}
