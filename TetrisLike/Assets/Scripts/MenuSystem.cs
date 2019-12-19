using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{

    public Text _levelText;
    public Text _highScoreText;
    public Text _lastScore;

    void Start()
    {
        //Every if statement checks if text is referenced to any object avoiding the error notification in console
        if(_levelText != null)
        {
            _levelText.text = "0";
        }
        if(_highScoreText != null)
        {
            _highScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
        }
        if(_lastScore != null)
        {
            _lastScore.text = PlayerPrefs.GetInt("LastScore").ToString();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Game Menu Functions
    /// </summary>
    public void StartLevel()
    {
        if(Game._startingLevel == 0)
        {
            Game._startingAtLevelZero = true;
        }
        else
        {
            Game._startingAtLevelZero = false;
        }
        SceneManager.LoadScene("Main");
    }

    public void ChangedValue(float _value)
    {
        Game._startingLevel = (int)_value;
        _levelText.text = _value.ToString();
    }

    /// <summary>
    /// Game over Scene Functions
    /// </summary>
	public void RestartLevel()
    {
        SceneManager.LoadScene("Main");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("GameMenu");
    }
}
