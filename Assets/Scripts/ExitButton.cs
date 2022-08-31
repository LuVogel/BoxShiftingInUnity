using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private bool exitIsPressed = false;

    private void OnMouseDown() {
        exitIsPressed = true;
    }

    public bool IsExitPressed() {
        if (exitIsPressed) {
            exitIsPressed = false;
            return true;
        }
        return false;
    }
}
