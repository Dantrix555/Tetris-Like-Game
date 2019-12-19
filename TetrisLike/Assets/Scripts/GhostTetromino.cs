using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTetromino : MonoBehaviour
{

	// Use this for initialization
	void Start()
    {
        tag = "CurrentGhostTetromino";
        foreach(Transform _mino in transform)
        {
            _mino.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
        }
	}
	
	// Update is called once per frame
	void Update()
    {
        FollowActiveTetromino();
        MoveDown();
	}

    void FollowActiveTetromino()
    {
        
        Transform _currentActiveTetrominoTransform = GameObject.FindGameObjectWithTag("CurrentActiveTetromino").transform;
        transform.position = _currentActiveTetrominoTransform.position;
        transform.rotation = _currentActiveTetrominoTransform.rotation;
    }

    void MoveDown()
    {
        while(CheckIsValidPosition())
        {
            transform.position += new Vector3(0, -1, 0);
        }
        if(!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
        }
    }

    bool CheckIsValidPosition()
    {
        foreach(Transform _mino in transform)
        {
            Vector2 _position = FindObjectOfType<Game>().Round(_mino.position);
            if(FindObjectOfType<Game>().CheckIsInsideGrid(_position) == false)
            {
                return false;
            }
            if(FindObjectOfType<Game>().GetTransformAtGridPosition(_position) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(_position).parent.tag == "CurrentActiveTetromino")
            {
                return true;
            }
            if(FindObjectOfType<Game>().GetTransformAtGridPosition(_position) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(_position).parent != transform)
            {
                return false;
            }
        }
        return true;
    }
}
