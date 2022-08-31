using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private bool homeIsPressed = false;

    private void OnMouseDown() {
        homeIsPressed = true;
    }

    public bool IsHomePressed() {
        if (homeIsPressed) {
            homeIsPressed = false;
            return true;
        }
        return false;
    }
}
