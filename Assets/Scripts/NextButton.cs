using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private bool nextIsPressed = false;

    private void OnMouseDown() {
        nextIsPressed = true;
    }

    public bool IsNextPressed() {
        if (nextIsPressed) {
            nextIsPressed = false;
            return true;
        }
        return false;
    }
}
