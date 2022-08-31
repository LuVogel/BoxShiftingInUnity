using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using System.IO;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{

    [Header("Prefabs:")]
    [SerializeField] private MovingSquare _movingPrefab; 
    [SerializeField] private EnemySquare _enemyPrefab; 
    [SerializeField] private WiningSquare _winingSquarePrefab;
    [SerializeField] private ResetButton _resetPrefab;
    [SerializeField] private StartManager _startScreenPrefab;
    [SerializeField] private StartButton _startButtonPrefab;
    [SerializeField] private WiningScreen _winingScreenPrefab;
    [SerializeField] private HomeButton _homeButtonPrefab;
    [SerializeField] private NextButton _nextButtonPrefab;
    [SerializeField] private ExitButton _exitButtonPrefab;
    [SerializeField] private EndingScreen _endingScreenPrefab;
    [SerializeField] private LoadButton _loadButtonPrefab;
    [SerializeField] private Teleporter _teleportersPrefab;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Animator _animatorPrefab;

    [Space]
    [Header("Audio:")]
    [SerializeField] private AudioClip[] _moveClips;
    [SerializeField] private AudioClip[] _winClip;
    [SerializeField] private AudioSource _source;

    [Space]
    [Header("Spawned Objects:")]
    
    private Teleporter tp1;
    private Teleporter tp2;
    private LoadButton spawnLoadButton;
    private EndingScreen spawnEndingScreen;
    private ExitButton spawnExitButton;
    private HomeButton spawnHomeButton;
    private NextButton spawnNextButton;
    private WiningSquare winingCondition;
    private ResetButton spawnReset;
    private StartButton spawnStartButton;
    private WiningScreen spawnWiningScreen;
    private StartManager spawnStartManager;
    private MovingSquare _spawnedMovingSquare;

    [Space]
    [Header("Lists:")]
    private List<Teleporter> tpList;
    private List<Tile> _tiles;
    private List<string[]> levelLines;
    private List<EnemySquare> _spawnedEnemySquare;
    private List<Tile> tilesToMove;

    [Space]
    [Header("Camera and Game")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private Transform _cam;
    [SerializeField] private int _level = 1;
    [SerializeField] Vector2 resetPos;
    [SerializeField] Vector2 Worldpos2D;
    private GameState _state;
    [SerializeField] private bool inGame; 
    private bool won = false;  


    [Space]
    [Header("Touch for Android:")]
    private Vector2 startTouchPos;
    private Vector2 currentTouchPos;
    private Vector2 endTouchPos;
    private bool stopTouch = false;
    public float swipeRange = 50.0f;

    
   


    // Standart Methods (MonoBehaviour)
    //**********************************************

    void Start() {
        ChangeState(GameState.GenerateStartScreen);   
    }

   void Update() {
       if (_state != GameState.WaitingInput) {
           return;
       }
       if (spawnStartButton != null && spawnStartButton.IsStartPressed()) {
           _level = 1;
           ClearStartScreen();
           ChangeState(GameState.GenerateGrid);
       }
       if (spawnReset != null && spawnReset.ResetIsPressed()) {
           if (won) {
               _level -= 1;
               WritePlayerLevelToFile();
           }
           won = false;
           ClearAfterReset();
           ChangeState(GameState.SpawningPlayers);
       }
       if (spawnExitButton != null && spawnExitButton.IsExitPressed()) {
           Application.Quit();
       }
       if (spawnHomeButton != null && spawnHomeButton.IsHomePressed()) {
           ClearAfterHomePress();
           ChangeState(GameState.GenerateStartScreen);
       }
       if (spawnLoadButton != null && spawnLoadButton.IsLoadPressed()) {
           _level = GetPlayerLevel();
           ClearStartScreen();
           ChangeState(GameState.GenerateGrid);
       }
       if (spawnNextButton != null && spawnNextButton.IsNextPressed()) {
           _source.Stop();
           won = false;
           ClearAfterNextPress();
           ChangeState(GameState.GenerateLevel);
       }
       if (Application.platform == RuntimePlatform.WindowsPlayer || 
                Application.platform == RuntimePlatform.WindowsEditor) {
            if (Input.GetKeyDown(KeyCode.UpArrow) && inGame) {
                MovePlayer(Vector2.up);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && inGame) {
                MovePlayer(Vector2.down);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && inGame) {
                MovePlayer(Vector2.left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) && inGame) {
                MovePlayer(Vector2.right);
            }
            if (Input.GetKeyDown(KeyCode.F12) && inGame) {
                    ChangeState(GameState.Win);
            }
       }
   }
   
    // handle GameState
    //***********************************************

   private void ChangeState(GameState newState) {
        _state = newState;

        switch (newState) {
            case GameState.GenerateStartScreen:
                GenerateStartScreen();
                break;
            case GameState.GenerateGrid:
                GenerateGrid();
                break;
            case GameState.GenerateLevel:
                GenerateLevel();
                break;
            case GameState.SpawningPlayers:
                SpawnPlayers();
                break;
            case GameState.SpawningEnemies:
                SpawnEnemies();
                break;
            case GameState.SpawningWiningSquare:
                SpawnWiningField();
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                GenerateWiningScreen();
                break;
            case GameState.Lose:
                break;
            case GameState.GenerateEndingScreen:
                GenerateEndingScreen();
                break;
            case GameState.SpawnTeleporters:
                SpawnTeleporters();
                break;
        }
    }

    public enum GameState {
        GenerateLevel,
        SpawningPlayers,
        SpawningEnemies,
        SpawningWiningSquare,
        WaitingInput,
        Moving,
        Win,
        Lose,
        GenerateStartScreen,
        GenerateGrid,
        GenerateEndingScreen,
        SpawnTeleporters
    }

    // Movement
    //******************************************

   void MovePlayer(Vector2 dir) {
        ChangeState(GameState.Moving);
        var tempTile = _spawnedMovingSquare;
        var next = tempTile._tile;
        _source.PlayOneShot(_moveClips[Random.Range(0, _moveClips.Length)], 0.2f);
        do {
            tempTile.SetBlock(next); 
            var possibleTile = GetTileAtPosition(next.pos + dir);
            if (possibleTile != null) {
                if (possibleTile.OccupiedPlayer == null && possibleTile.OccupiedEnemy == null) {
                    if (possibleTile.OccupiedTeleporter == null) {
                        next = possibleTile;
                    } else {
                        if (possibleTile.pos.x == tp1.pos.x && possibleTile.pos.y == tp1.pos.y) {
                            next = tp2._tile;
                        } else {
                            next = tp1._tile;
                        }
                    }
                }
            } 
            tempTile.transform.position = tempTile._tile.pos;
        } while (next != tempTile._tile);
        ChangeState(((tempTile._tile.pos.x == winingCondition.pos.x) && (tempTile.pos.y == winingCondition.pos.y)) ? GameState.Win : GameState.WaitingInput);
   }

   Tile GetTileAtPosition(Vector2 pos) {
       return _tiles.FirstOrDefault(n => n.pos == pos);
   }


   // Mobile Touch
   //********************************************************

   // returns swipedirection:
   // 0 = left, 1 = right, 2 = up, 3 = down, -1 == error
   public int Swipe() {
       if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
           startTouchPos = Input.GetTouch(0).position;
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
            currentTouchPos = Input.GetTouch(0).position;
            Vector2 distance = currentTouchPos - startTouchPos;
            if (!stopTouch) {
                if (distance.x < -swipeRange) {
                    return 0;
                } else if (distance.x > swipeRange) {
                    return 1;
                } else if (distance.y > swipeRange) {
                   return 2;
                } else if (distance.y < -swipeRange) {
                   return 3;
                }
            } 
        }
        return -1;
   }
    

    // GenerateFields and Screens
    //*******************************************************


   void GenerateStartScreen() {
        spawnStartManager = Instantiate(_startScreenPrefab, new Vector3(_width / 2, _height / 2), Quaternion.identity);
        spawnStartManager.name = $"StartScreen {_width} {_height}";
        spawnStartButton = Instantiate(_startButtonPrefab, new Vector3(_width / 2, _height / 2 + 1), Quaternion.identity);
        spawnStartButton.name = $"StartButton";
        spawnExitButton = Instantiate(_exitButtonPrefab, new Vector3(0, _height), Quaternion.identity);
        spawnExitButton.name = $"ExitButton";
        spawnLoadButton = Instantiate(_loadButtonPrefab, new Vector3(_width / 2, _height / 2 -1), Quaternion.identity);
        spawnLoadButton.name = $"LoadButton";
        _cam.transform.position = new Vector3((float) _width/2 -0.5f, (float) _height/2 -0.5f,-10);
        inGame = false;
        ChangeState(GameState.WaitingInput);
    }

    void GenerateWiningScreen() {
        _source.PlayOneShot(_winClip[Random.Range(0, _winClip.Length)], 0.2f);
        spawnWiningScreen = Instantiate(_winingScreenPrefab, new Vector3(_width / 2,_height / 2), Quaternion.identity);
        spawnWiningScreen.name = $"WiningScreen";
        spawnNextButton = Instantiate(_nextButtonPrefab, new Vector3(_width - 2, _height), Quaternion.identity);
        inGame = false;
        _level += 1;
        won = true;
        WritePlayerLevelToFile();
        ChangeState(GameState.WaitingInput);
    }

    void GenerateEndingScreen() {
        spawnEndingScreen = Instantiate(_endingScreenPrefab, new Vector3(_width / 2, _height / 2), Quaternion.identity);
        spawnEndingScreen.name = $"EndingScreen";
        inGame = false;
        ChangeState(GameState.WaitingInput);
    }

    void GenerateGrid() {
        _tiles = new List<Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x,y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
                _tiles.Add(spawnedTile);
             }
        }
        spawnReset = Instantiate(_resetPrefab, new Vector3(_width, _height), Quaternion.identity);
        spawnReset.name = $"Reset {_width} {_height}";
        spawnHomeButton = Instantiate(_homeButtonPrefab, new Vector3(_width -1, _height), Quaternion.identity);
        spawnHomeButton.name = $"HomeButton";
                
        //spawn MovingSquare
        ChangeState(GameState.GenerateLevel);
    }

    // Level Handler
    // **************************************
    void readFileForLevel() {
        string file = Resources.Load<TextAsset>(@"Levels/Level"+_level).text;
        string[] fileLines = file.Split('\n');
        levelLines = new List<string[]>();
        char[] separator = {','};
        char[] separatorSpace = {' '};
        // add enemy line
        levelLines.Add(fileLines[0].Split(separator));
        // add player line
        levelLines.Add(fileLines[1].Split(separatorSpace));
        // add wining line
        levelLines.Add(fileLines[2].Split(separatorSpace));
        if (_level >=15) {
            levelLines.Add(fileLines[3].Split(separator));
        }
    }

    void GenerateLevel() {
        if (_level == 21) {
            ClearForEndingScreen();
            ChangeState(GameState.GenerateEndingScreen);
        } else {
            ClearOccupiedTiles();
            readFileForLevel();
            ChangeState(GameState.SpawningEnemies);
        }
    }

    int GetPlayerLevel() {
        string filePath = Application.streamingAssetsPath + "/PlayerLevel/playerLevel.txt";
        string pLevel = File.ReadAllText(filePath);
        return int.Parse(pLevel);
    }

    void WritePlayerLevelToFile() {
        if (!Directory.Exists(Application.streamingAssetsPath + "/PlayerLevel/")) {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/PlayerLevel/");
        }
        string filePath = Application.streamingAssetsPath + "/PlayerLevel/playerLevel.txt";
        File.WriteAllText(filePath, _level.ToString());
        
    }

    //Spawn Player, Enemies and other Blocks
    //****************************************************************


    void SpawnPlayers() {
        string[] playerLine = levelLines[1];
        var tileTemp = _tiles.Where(n => n.pos.x == int.Parse(playerLine[0]) && n.pos.y == int.Parse(playerLine[1])).ToList();
        foreach (var tempSpawnTile in tileTemp.Take(1)) {
            SpawnPlayer(tempSpawnTile);
        }
        inGame = true;
        ChangeState(GameState.WaitingInput);
    }

    // only one player possible to spawn
    void SpawnPlayer(Tile tile) {
        var spawnedMovingSquare = Instantiate(_movingPrefab, tile.pos, Quaternion.identity);
        var playerX = tile.pos.x;
        var playerY = tile.pos.y;
        spawnedMovingSquare.name =$"MovingSquare {playerX} {playerY}";
        spawnedMovingSquare.SetBlock(tile);
        _spawnedMovingSquare = spawnedMovingSquare;
    }

    void SpawnEnemies() {
        _spawnedEnemySquare = new List<EnemySquare>();
        string[] enemyLine = levelLines[0];
        foreach (string s in enemyLine) {
            char[] sep = {' '};
            string[] spawnLine = s.Split(sep);
            var tileTemp = _tiles.Where((n, p) => n.pos.x == int.Parse(spawnLine[0]) && n.pos.y == int.Parse(spawnLine[1])).ToList();
            foreach (var tempSpawnTile in tileTemp) {
                SpawnEnemy(tempSpawnTile);
            }
        }
    
        
        ChangeState(GameState.SpawningWiningSquare);
    }

    void SpawnEnemy(Tile tile) {
        var spawnedEnemySquare = Instantiate(_enemyPrefab, tile.pos, Quaternion.identity);
        var x1 = tile.pos.x;
        var y1 = tile.pos.y;
        spawnedEnemySquare.name = $"EnemySquare {x1} {y1}";
        spawnedEnemySquare.SetBlock(tile);
        _spawnedEnemySquare.Add(spawnedEnemySquare);

    }

    // only one wininig field possible
    void SpawnWiningField() {
        string[] winingLine = levelLines[2];
        var tileTemp = _tiles.Where(n => n.pos.x == int.Parse(winingLine[0]) && n.pos.y == int.Parse(winingLine[1])).ToList();
        var tempSpawnTile = tileTemp[0];
        var spawnedWinigSquare = Instantiate(_winingSquarePrefab, tempSpawnTile.pos, Quaternion.identity);
        var x1 = tempSpawnTile.pos.x;
        var y1 = tempSpawnTile.pos.y;
        spawnedWinigSquare.name = $"WiningField {x1} {y1}";
        spawnedWinigSquare.SetBlock(tempSpawnTile);
        winingCondition = spawnedWinigSquare;
        if (_level >= 15) {
            ChangeState(GameState.SpawnTeleporters);
        } else {
            ChangeState(GameState.SpawningPlayers);  
        } 
    }

    void SpawnTeleporters() {
        string[] tpLine = levelLines[3];
        foreach (string s in tpLine) {
            char[] sep = {' '};
            string[] tempLine = s.Split(sep);
            var tileTemp = _tiles.Where((n, p) => n.pos.x == int.Parse(tempLine[0]) && n.pos.y == int.Parse(tempLine[1])).ToList();
            foreach (var tempSpawnTile in tileTemp) {
                SpawnTeleporter(tempSpawnTile);
            }
        }
        ChangeState(GameState.SpawningPlayers);
    }

    void SpawnTeleporter(Tile tile) {
        var spawnedTeleporter = Instantiate(_teleportersPrefab, tile.pos, Quaternion.identity);
        var x1 = tile.pos.x;
        var y1 = tile.pos.y;
        spawnedTeleporter.name = $"Teleporter {x1} {y1}";
        spawnedTeleporter.SetBlock(tile);
        if (tp1 == null) {
            Debug.Log("spawning tp1");
            tp1 = spawnedTeleporter;
        } else {
            Debug.Log("spawning tp2");
            tp2 = spawnedTeleporter;
        }
    }

    // Destroy Objects 
    //**************************************************************
    
    void ClearAfterReset() {
        ClearPlayer();
        ClearWinScreenAndNextButton();
    }

    void ClearTeleporters() {
        if (tp1 != null) {
            Debug.Log("destroy tp1");
            Destroy(tp1.gameObject);
            tp1 = null;
        }
        if (tp2 != null) {
            Debug.Log("destroy tp2");
            Destroy(tp2.gameObject);
            tp2 = null;
        }
    }

    void ClearWinScreenAndNextButton() {
        if (spawnWiningScreen != null) {
            Destroy(spawnWiningScreen.gameObject);
        Destroy(spawnNextButton.gameObject);
        }
        
    }
    void ClearPlayer() {
        if (_spawnedMovingSquare != null) {
            Destroy(_spawnedMovingSquare.gameObject);
        }
        
    }
    void ClearEnemies() {
        if (_spawnedEnemySquare != null) {
            foreach (EnemySquare enTemp in _spawnedEnemySquare) {
                Destroy(enTemp.gameObject);
            }
        }
        _spawnedEnemySquare = null;
    }
    void ClearTiles() {
        if (_tiles != null) {
            foreach (Tile tiTemp in _tiles) {
                Destroy(tiTemp.gameObject);
            }
        }
        _tiles = null;
    }
    
    void ClearAfterHomePress() {
        ClearAfterReset();
        ClearEnemies();
        ClearResetButton();
        ClearWiningField();
        ClearTiles();
        ClearHomeButton();
        ClearEndingScreen();
        ClearTeleporters();

    }

    void ClearEndingScreen() {
        if (spawnEndingScreen != null) {
            Destroy(spawnEndingScreen.gameObject);
        }
    }
    void ClearResetButton() {
        if (spawnReset != null) {
            Destroy(spawnReset.gameObject);
        }
        
    }
    void ClearWiningField() {
        if (winingCondition != null) {
            Destroy(winingCondition.gameObject);
        }
    }

    void ClearStartScreen() {
        Destroy(spawnLoadButton.gameObject);
        Destroy(spawnStartButton.gameObject);
        Destroy(spawnStartManager.gameObject);
    }

    void ClearAfterNextPress() {
        ClearWiningField();
        ClearWinScreenAndNextButton();
        ClearPlayer();
        ClearEnemies();
        ClearTeleporters();
    }

    void ClearOccupiedTiles() {
        foreach (Tile tile in _tiles) {
            tile.OccupiedEnemy = null;
            tile.OccupiedPlayer = null;
            tile.WiningField = null;
            tile.OccupiedTeleporter = null;
        }
    }

    void ClearHomeButton() {
        if (spawnHomeButton != null) {
            Destroy(spawnHomeButton.gameObject);
        }
        
    }

    void ClearForEndingScreen(){
        ClearAfterReset();
        ClearEnemies();
        ClearResetButton();
        ClearWiningField();
        ClearTiles();
        ClearTeleporters();
    }
}
