using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private bool startIsPressed = false;

    

    private void OnMouseDown() {
        startIsPressed = true;
    }

    public bool IsStartPressed() {
        if (startIsPressed) {
            startIsPressed = false;
            return true;
        }
        return false;
    }
}
