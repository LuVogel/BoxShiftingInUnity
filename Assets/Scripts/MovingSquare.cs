using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSquare : MonoBehaviour {

    [SerializeField] private SpriteRenderer _renderer;
    public Vector2 pos => transform.position;
    public Tile _tile;

    public void SetBlock(Tile currentTile) {
        if (_tile != null) {
            _tile.OccupiedPlayer = null;
        }
        _tile = currentTile;
        _tile.OccupiedPlayer = this;
    }
}

