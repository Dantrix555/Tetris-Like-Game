using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetromino : MonoBehaviour
{

    private float _fall = 0;
    private float _fallSpeed;
    public bool _allowRotation = true;
    
    //limit rotation is used for some pieces, to control the rotation, in case the piece can also affect it normal position after rotation
    public bool _limitRotation = false;
    [SerializeField]
    private bool _isPlaced = false;
    private int _individualScore = 100;
    private float _individualScoreTime;

    //Audio Resources for the tetromino
    public AudioClip _rotationSound;
    public AudioClip _landedSound;
    public AudioClip _movingSound;
    private AudioSource _tetrominoAudioSource;
    private float _continuousVerticalSpeed = 0.05f;
    private float _continuousHorizontalSpeed = 0.1f;
    private float _buttonDownWaitMax = 0.2f;
    private float _verticalTimer = 0;
    private float _horizontalTimer = 0;
    private float _buttonDownWaitTimerHorizontal = 0;
    private float _buttonDownWaitTimerVertical = 0;
    private bool _moveInmediateHorizontal = false;
    private bool _moveInmediateVertical = false;

    //Touch Movement variables
    private int _touchSensivityHorizontal = 8;
    private int _touchSensivityVertical = 4;
    Vector2 _previousUnitPosition = Vector2.zero;
    Vector2 _direction = Vector2.zero;
    bool _moved = false;

    // Use this for initialization
    void Start ()
    {
        _tetrominoAudioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(!Game._isPaused)
        {
            UpdateFallSpeed();
            CheckUserInput();
            UpdateIndividualScore();
        }
	}

    void UpdateFallSpeed()
    {
        _fallSpeed = Game._fallSpeed;
    }

    void UpdateIndividualScore()
    {
        if(_individualScoreTime < 1)
        {
            _individualScoreTime += Time.deltaTime;
        }
        else
        {
            _individualScoreTime = 0;
            _individualScore = Mathf.Max(_individualScore - 10, 0);
        }
    }

    void CheckUserInput()
    {

        #if UNITY_IOS

        if(Input.touchCount > 0)
        {
            Touch _touch = Input.GetTouch(0);
            if(_touch.phase == TouchPhase.Began)
            {
                _previousUnitPosition = new Vector2(_touch.position.x, _touch.position.y);
            }
            else if(_touch.phase == TouchPhase.Moved)
            {
                Vector2 _touchDeltaPosition = _touch.deltaPosition;
                _direction = _touchDeltaPosition.normalized;
                if(Mathf.Abs(_touch.position.x - _previousUnitPosition.x) >= _touchSensivityHorizontal && _direction.x < 0 && _touch.deltaPosition.y > -10
                    && _touch.deltaPosition.y < 10)
                {
                    MoveLeft();
                    _previousUnitPosition = _touch.position;
                    _moved = true;
                }
                else if(Mathf.Abs(_touch.position.x - _previousUnitPosition.x) >= _touchSensivityHorizontal && _direction.x > 0 && _touch.deltaPosition.y > -10
                    && _touch.deltaPosition.y < 10)
                {
                    MoveRight();
                    _previousUnitPosition = _touch.position;
                    _moved = true;
                }
                else if(Mathf.Abs(_touch.position.y - _previousUnitPosition.y) >= _touchSensivityVertical && _direction.y < 0 && _touch.deltaPosition.x > -10
                    && _touch.deltaPosition.x < 10)
                {
                    MoveDown();
                    _previousUnitPosition = _touch.position;
                    _moved = true;
                }
            }
            else if(_touch.phase == TouchPhase.Ended)
            {
                if(!_moved && _touch.position.x > Screen.width / 4)
                {
                    Rotate();
                }
                _moved = false;
            }
        }
        if(Time.time - _fall >= _fallSpeed)
        {
            MoveDown();
        }
    
        #else

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _horizontalTimer = 0;
            _buttonDownWaitTimerHorizontal = 0;
            _moveInmediateHorizontal = false;
        }

        if(Input.GetKeyUp(KeyCode.DownArrow))
        {
            _verticalTimer = 0;
            _buttonDownWaitTimerVertical = 0;
            _moveInmediateVertical = false;
        }

        if(Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        //Time.time property is used to count every second during gameplay, fallspeed is the value of the amount of units the piece will fall down
        if(Input.GetKey(KeyCode.DownArrow) || Time.time - _fall >= _fallSpeed)
        {
            MoveDown();
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            Rotate();
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {
            SlamDown();
        }

        #endif

    }

    public void SlamDown()
    {
        while(CheckIsValidPosition())
        {
            transform.position += new Vector3(0, -1, 0);
        }
        if(!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
            FindObjectOfType<Game>().UpdateGrid(this);
            FindObjectOfType<Game>().DeleteRow();
            if (FindObjectOfType<Game>().CheckIsAboveGrid(this))
            {
                _individualScore = 0;
                FindObjectOfType<Game>().GameOver();
            }
            enabled = false;
            Game._currentScore += _individualScore;
            PlayClip(_landedSound);
            FindObjectOfType<Game>().UpdateHighScore();
            FindObjectOfType<Game>().SpawnTetromino();
            tag = "Untagged";
        }
    }

    void MoveLeft()
    {
        if (_moveInmediateHorizontal)
        {
            if (_buttonDownWaitTimerHorizontal < _buttonDownWaitMax)
            {
                _buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }

            if (_horizontalTimer < _continuousHorizontalSpeed)
            {
                _horizontalTimer += Time.deltaTime;
                return;
            }
        }
        if (!_moveInmediateHorizontal)
        {
            _moveInmediateHorizontal = true;
        }

        _horizontalTimer = 0;

        transform.position += new Vector3(-1, 0, 0);

        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);
            PlayClip(_movingSound);
        }
        else
        {
            transform.position += new Vector3(1, 0, 0);
        }
    }

    void MoveRight()
    {
        if (_moveInmediateHorizontal)
        {
            if (_buttonDownWaitTimerHorizontal < _buttonDownWaitMax)
            {
                _buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }

            if (_horizontalTimer < _continuousHorizontalSpeed)
            {
                _horizontalTimer += Time.deltaTime;
                return;
            }
        }
        if (!_moveInmediateHorizontal)
        {
            _moveInmediateHorizontal = true;
        }

        _horizontalTimer = 0;

        transform.position += new Vector3(1, 0, 0);

        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);
            PlayClip(_movingSound);
        }
        else
        {
            transform.position += new Vector3(-1, 0, 0);
        }
    }

    void MoveDown()
    {
        if (_moveInmediateVertical)
        {
            if (_buttonDownWaitTimerVertical < _buttonDownWaitMax)
            {
                _buttonDownWaitTimerVertical += Time.deltaTime;
                return;
            }

            if (_verticalTimer < _continuousVerticalSpeed)
            {
                _verticalTimer += Time.deltaTime;
                return;
            }
        }
        if (!_moveInmediateVertical)
        {
            _moveInmediateVertical = true;
        }

        _verticalTimer = 0;

        transform.position += new Vector3(0, -1, 0);

        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);
            if (Input.GetKey(KeyCode.DownArrow))
            {
                PlayClip(_movingSound);
            }
        }
        else
        {
            transform.position += new Vector3(0, 1, 0);
            FindObjectOfType<Game>().DeleteRow();
            if (FindObjectOfType<Game>().CheckIsAboveGrid(this))
            {
                _individualScore = 0;
                FindObjectOfType<Game>().GameOver();
            }
            enabled = false;
            Game._currentScore += _individualScore;
            PlayClip(_landedSound);
            FindObjectOfType<Game>().UpdateHighScore();
            FindObjectOfType<Game>().SpawnTetromino();
            tag = "Untagged";
        }
        //We need to update the fall value to the actual, if is needed to fall in the next second
        _fall = Time.time;
    }

    void Rotate()
    {
        if (_allowRotation)
        {
            if (_limitRotation)
            {
                if (transform.rotation.eulerAngles.z >= 90)
                {
                    transform.Rotate(0, 0, -90);
                }
                else
                {
                    transform.Rotate(0, 0, 90);
                }
            }
            else
            {
                transform.Rotate(0, 0, 90);
            }
            if (CheckIsValidPosition())
            {
                FindObjectOfType<Game>().UpdateGrid(this);
                PlayClip(_rotationSound);
            }
            else
            {
                if (_limitRotation)
                {
                    if (transform.rotation.eulerAngles.z >= 90)
                    {
                        transform.Rotate(0, 0, -90);
                    }
                    else
                    {
                        transform.Rotate(0, 0, 90);
                    }
                }
                else
                {
                    transform.Rotate(0, 0, -90);
                }
            }
        }
    }

    bool CheckIsValidPosition()
    {
        //_mino = piece, but this is only the transform of the piece (GameObject)
        foreach(Transform _mino in transform)
        {
            Vector2 _position = FindObjectOfType<Game>().Round(_mino.position);
            if(!FindObjectOfType<Game>().CheckIsInsideGrid(_position))
            {
                return false;
            }
            if(FindObjectOfType<Game>().GetTransformAtGridPosition(_position) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(_position).parent != transform)
            {
                return false;
            }
        }
        return true;
    }

    void PlayClip(AudioClip _clip)
    {
        _tetrominoAudioSource.PlayOneShot(_clip);
    }
}
