using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static int _gridWidth = 10;
    public static int _gridHeight = 20;
    public static Transform[,] _grid = new Transform[_gridWidth, _gridHeight];
    public static int _currentScore = 0;
    public static float _fallSpeed = 1f;
    public static bool _startingAtLevelZero;
    public static int _startingLevel;
    public static bool _isPaused = false;
    public int _currentLevel = 0;
    private int _numberOfLinesCleared = 0;
    public int _scoreOneLine = 40;
    public int _scoreTwoLines = 100;
    public int _scoreThreeLines = 300;
    public int _scoreFourLines = 1200;
    private int _numberOfRowsThisTurn = 0;
    public Text _hudScore;
    public Text _hudLevel;
    public Text _hudLines;
    private AudioSource _gameAudioSource;
    public AudioClip _rowCompletedSound;
    private GameObject _previewTetromino;
    private GameObject _nextTetromino;
    private GameObject _savedTetromino;
    private GameObject _ghostTetromino;
    private bool _gameStarted = false;
    private int _startingHighScore;
    private Vector2 _previewTetrominoPosition = new Vector2(-6.5f, 16);
    private Vector2 _savedTetrominoPosition = new Vector2(-6.5f, 10);
    public int _maxSwaps = 2;
    private int _currentSwaps = 0;
    public Canvas _hudCanvas;
    public Canvas _pauseCanvas;
    public GameObject _pauseMenuObject;

    // Use this for initialization
    void Start()
    {
        SpawnTetromino();
        _gameAudioSource = GetComponent<AudioSource>();
        _currentLevel = _startingLevel;
        _startingHighScore = PlayerPrefs.GetInt("HighScore");
    }

    void Update()
    {
        UpdateScore();
        UpdateScoreHUD();
        UpdateLevel();
        UpdateSpeed();
        CheckUserInput();
    }

    void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (Time.timeScale == 1)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
        if(Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            GameObject _tempNextTetromino = GameObject.FindGameObjectWithTag("CurrentActiveTetromino");
            SaveTetromino(_tempNextTetromino.transform);
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        _isPaused = true;
        _gameAudioSource.Pause();
        _pauseCanvas.enabled = true;
        _hudCanvas.enabled = false;
        _pauseMenuObject.SetActive(true);
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
        _isPaused = false;
        _gameAudioSource.Play();
        _pauseCanvas.enabled = false;
        _hudCanvas.enabled = true;
        _pauseMenuObject.SetActive(false);
    }

    void UpdateLevel()
    {
        if(_startingAtLevelZero || (!_startingAtLevelZero && _numberOfLinesCleared / 10 > _startingLevel))
        {
            _currentLevel = _numberOfLinesCleared / 10;
        }
    }

    void UpdateSpeed()
    {
        _fallSpeed = 1.0f - _currentLevel * 0.1f;
    }

    public void UpdateScore()
    {
        if (_numberOfRowsThisTurn > 0)
        {
            if(_numberOfRowsThisTurn == 1)
            {
                ClearedOneLine();
            }
            else if(_numberOfRowsThisTurn == 2)
            {
                ClearedTwoLines();
            }
            else if(_numberOfRowsThisTurn == 3)
            {
                ClearedThreeLines();
            }
            else if(_numberOfRowsThisTurn == 4)
            {
                ClearedFourLines();
            }
            _numberOfRowsThisTurn = 0;
            UpdateHighScore();
            PlayClip(_rowCompletedSound);
        }
        PlayerPrefs.SetInt("LastScore", _currentScore);
    }

    public void UpdateScoreHUD()
    {
        _hudScore.text = _currentScore.ToString();
        _hudLevel.text = _currentLevel.ToString();
        _hudLines.text = _numberOfLinesCleared.ToString();
    }

    public void ClearedOneLine()
    {
        _currentScore += _scoreOneLine + (_currentLevel * 20);
        _numberOfLinesCleared++;
    }

    public void ClearedTwoLines()
    {
        _currentScore += _scoreTwoLines;
        _numberOfLinesCleared += 2;
    }

    public void ClearedThreeLines()
    {
        _currentScore += _scoreThreeLines;
        _numberOfLinesCleared += 3;
    }

    public void ClearedFourLines()
    {
        _currentScore += _scoreFourLines;
        _numberOfLinesCleared += 4;
    }

    public void UpdateHighScore()
    {
        if(_currentScore > _startingHighScore)
        {
            PlayerPrefs.SetInt("HighScore", _currentScore);
        }
    }

    bool CheckIsValidPosition(GameObject _tetromino)
    {
        foreach(Transform _mino in _tetromino.transform)
        {
            Vector2 _position = Round(_mino.position);
            if(!CheckIsInsideGrid(_position))
            {
                return false;
            }
            if(GetTransformAtGridPosition(_position) != null && GetTransformAtGridPosition(_position).parent != _tetromino.transform)
            {
                return false;
            }
        }
        return true;
    }

    public bool CheckIsAboveGrid(Tetromino _tetromino)
    {
        int x;
        for(x = 0; x < _gridWidth; ++x)
        {
            foreach(Transform _mino in _tetromino.transform)
            {
                Vector2 _position = Round(_mino.position);
                if(_position.y > _gridHeight - 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

	public bool IsFullRowAt(int y)
    {
        int x;
        for(x = 0; x < _gridWidth; ++x)
        {
            if(_grid[x,y] == null)
            {
                return false;
            }
        }
        _numberOfRowsThisTurn++;
        return true;
    }

    public void DeleteMinoAt(int y)
    {
        int x;
        for(x = 0; x < _gridWidth; ++x)
        {
            Destroy(_grid[x,y].gameObject);
            _grid[x, y] = null;
        }
    }

    public void MoveRowDown(int y)
    {
        int x;
        for(x = 0; x < _gridWidth; ++x)
        {
            if(_grid[x,y] != null)
            {
                _grid[x, y - 1] = _grid[x,y];
                _grid[x, y] = null;
                _grid[x, y - 1].position += new Vector3(0, -1, 0);
            }
        }
    }

    public void MoveAllRowsDown(int y)
    {
        int i;
        for(i = y; i < _gridHeight ; ++i)
        {
            MoveRowDown(i);
        }
    }

    public void DeleteRow()
    {
        int y;
        for(y = 0; y < _gridHeight; ++y)
        {
            if(IsFullRowAt(y))
            {
                DeleteMinoAt(y);
                MoveAllRowsDown(y + 1);
                --y;
            }
        }
    }

    public void UpdateGrid(Tetromino _tetromino)
    {
        int x, y;
        for(y = 0; y < _gridHeight; ++y)
        {
            for(x = 0; x < _gridWidth; ++x)
            {
                if(_grid[x,y] != null)
                {
                    if(_grid[x,y].parent == _tetromino.transform)
                    {
                        _grid[x, y] = null;
                    }
                }
            }
        }
        foreach(Transform _mino in _tetromino.transform)
        {
            Vector2 _position = Round(_mino.position);
            if(_position.y < _gridHeight)
            {
                _grid[(int)_position.x, (int)_position.y] = _mino;
            }
        }
    }

    public Transform GetTransformAtGridPosition(Vector2 _position)
    {
        if(_position.y > _gridHeight - 1)
        {
            return null;
        }
        else
        {
            return _grid[(int)_position.x, (int)_position.y];
        }
    }

    public void SpawnTetromino()
    {
        if(!_gameStarted)
        {
            _gameStarted = true;
            _nextTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), new Vector2(5.0f,20.0f), Quaternion.identity);
            _previewTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), _previewTetrominoPosition, Quaternion.identity);
            _previewTetromino.GetComponent<Tetromino>().enabled = false;
            _nextTetromino.tag = "CurrentActiveTetromino";
            SpawnGhostTetromino();
        }
        else
        {
            _previewTetromino.transform.localPosition = new Vector2(5.0f, 20.0f);
            _nextTetromino = _previewTetromino;
            _nextTetromino.GetComponent<Tetromino>().enabled = true;
            _nextTetromino.tag = "CurrentActiveTetromino";
            _previewTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), _previewTetrominoPosition, Quaternion.identity);
            _previewTetromino.GetComponent<Tetromino>().enabled = false;
            SpawnGhostTetromino();
        }
        _currentSwaps = 0;
    }

    public void SpawnGhostTetromino()
    {
        if(GameObject.FindGameObjectWithTag("CurrentGhostTetromino") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("CurrentGhostTetromino"));
        }
        _ghostTetromino = Instantiate(_nextTetromino, _nextTetromino.transform.position, Quaternion.identity);
        Destroy(_ghostTetromino.GetComponent<Tetromino>());
        _ghostTetromino.AddComponent<GhostTetromino>();
    }

    public void SaveTetromino(Transform  _tetromino)
    {
        _currentSwaps++;

        if (_currentSwaps > _maxSwaps)
        {
            return;
        }

        if(_savedTetromino != null)
        {
            GameObject _tempSavedTetromino = GameObject.FindGameObjectWithTag("CurrentSavedTetromino");
            _tempSavedTetromino.transform.localPosition = new Vector2(_gridWidth / 2, _gridHeight);
            if(!CheckIsValidPosition(_tempSavedTetromino))
            {
                _tempSavedTetromino.transform.localPosition = _savedTetrominoPosition;
                return;
            }

            _savedTetromino = Instantiate(_tetromino.gameObject);
            _savedTetromino.GetComponent<Tetromino>().enabled = false;
            _savedTetromino.transform.localPosition = _savedTetrominoPosition;
            _savedTetromino.tag = "CurrentSavedTetromino";

            _nextTetromino = Instantiate(_tempSavedTetromino);
            _nextTetromino.GetComponent<Tetromino>().enabled = true;
            _nextTetromino.transform.localPosition = new Vector2(_gridWidth / 2, _gridHeight);
            _nextTetromino.tag = "CurrentActiveTetromino";
            DestroyImmediate(_tetromino.gameObject);
            DestroyImmediate(_tempSavedTetromino);
            SpawnGhostTetromino();
        }
        else
        {
            _savedTetromino = Instantiate(GameObject.FindGameObjectWithTag("CurrentActiveTetromino"));
            _savedTetromino.GetComponent<Tetromino>().enabled = false;
            _savedTetromino.transform.localPosition = _savedTetrominoPosition;
            _savedTetromino.tag = "CurrentSavedTetromino";
            DestroyImmediate(GameObject.FindGameObjectWithTag("CurrentActiveTetromino"));
            SpawnTetromino();
        }
    }

    //This method avoid the piece to go outside the wall
    public bool CheckIsInsideGrid(Vector2 _position)
    {
        return ((int)_position.x >= 0 && (int)_position.x < _gridWidth && (int)_position.y > 0);
    }

    //This method is used to move the piece 1 by 1 in the x/y axis, which means avoid to move in decimal (but not integer) values
    public Vector2 Round(Vector2 _position)
    {
        return new Vector2(Mathf.Round(_position.x), Mathf.Round(_position.y));
    }

    private string GetRandomTetromino()
    {
        int _randomTetromino = Random.Range(1,8);
        string _randomTetrominoName = "Prefab/Tetromino_";

        switch(_randomTetromino)
        {
            case 1:
                _randomTetrominoName += "T";
                break;
            case 2:
                _randomTetrominoName += "Long";
                break;
            case 3:
                _randomTetrominoName += "Square";
                break;
            case 4:
                _randomTetrominoName += "J";
                break;
            case 5:
                _randomTetrominoName += "L";
                break;
            case 6:
                _randomTetrominoName += "S";
                break;
            case 7:
                _randomTetrominoName += "Z";
                break;
        }
        return _randomTetrominoName;
    }

    public void GameOver()
    {
        _currentScore = 0;
        UpdateScoreHUD();
        SceneManager.LoadScene("GameOver");
    }

    void PlayClip(AudioClip _clip)
    {
        _gameAudioSource.PlayOneShot(_clip);
    }
}
