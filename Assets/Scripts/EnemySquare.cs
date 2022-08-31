using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySquare : MonoBehaviour {

    [SerializeField] private SpriteRenderer _renderer;
    public Vector2 pos => transform.position;
    public Tile _tile;

    public void SetBlock(Tile currentTile) {
        if (_tile != null) {
            _tile.OccupiedEnemy = null;
        }
        _tile = currentTile;
        _tile.OccupiedEnemy = this;
    }
}
