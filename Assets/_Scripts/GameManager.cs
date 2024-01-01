using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<NumberBlock> _NumberBlocks;
    [SerializeField] private List<GameObject> _Levels;
    [SerializeField] private List<LevelButton> _LevelButtons;

    [Header("Panels")]
    public GameObject LevelSelectionPanel;
    public GameObject InLevelPanel;

    [Header("UI")]
    [SerializeField] private TMP_Text _levelNumberText;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _levelSelectionButton;
    [SerializeField] private Button _buttonRestart;
    [SerializeField] private Button _buttonQuit;
    [SerializeField] private GameObject _GameOverPanel;
    [SerializeField] private GameObject _CloseAppPanel;

    private RaycastHit2D _hitInfo;

    private Camera _camera;

    private NumberBlock _touchedBlock;

    public static bool isMovingBlock = false, levelsClearedOnce = false, isLevelSelected = false;

    public int _currentLevel = 0, _totalLevels;

    //Touch
    private Touch _touch;
    private Vector2 _touchStartPosition;
    private Vector2 _touchEndPosition;

    /// <summary>
    /// Player swiped direction
    /// </summary>
    public enum MoveDirection
    {
        MoveLeft, MoveRight, MoveUp, MoveDown
    }
    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(instance);

        _camera = Camera.main;
        _totalLevels = _Levels.Count;
        GlobalVariables.totalUnlockedLevels = PlayerPrefs.GetInt("CurrentLevel");

        for (int i = 0; i < _totalLevels; i++) GlobalVariables.LevelCleared.Add(false);
        //testing purpose
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("CurrentLevel"))
        {
            PlayerPrefs.SetInt("CurrentLevel", 0);
        } else
        {
            _currentLevel = PlayerPrefs.GetInt("CurrentLevel");
        }
        
        UpdateLevelNumber();
        InitNumberBlocks();


    }

    private void OnEnable()
    {
        _resetButton.onClick.AddListener(Reset);
        _levelSelectionButton.onClick.AddListener(SwitchToLevelSelectionMenu);
        _buttonRestart.onClick.AddListener(Restart);
        _buttonQuit.onClick.AddListener(Quit);
    }

    private void OnDisable()
    {
        _resetButton.onClick.RemoveListener(Reset);
        _levelSelectionButton.onClick.RemoveListener(SwitchToLevelSelectionMenu);
        _buttonRestart.onClick.RemoveListener(Restart);
        _buttonQuit.onClick.RemoveListener(Quit);
    }
    private void Update()
    {
        if(Input.touchCount > 0)
        {
            HandleTouchInput();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_CloseAppPanel.activeSelf)
            {
                _CloseAppPanel.SetActive(true);
                StartCoroutine(PopUpCloseAppPanel());
            }
            else
            {
                Application.Quit();
            }
        }
    }

    public void UpdateLevelNumber()
    {
        if (!isLevelSelected) {
            if (PlayerPrefs.HasKey("CurrentLevel")) PlayerPrefs.SetInt("CurrentLevel", _currentLevel);  //save current level 
        }

        _levelNumberText.text = (_currentLevel+1).ToString();

        _Levels[_currentLevel].SetActive(true);
        isLevelSelected = false;
    }

    private IEnumerator PopUpCloseAppPanel()
    {
        yield return new WaitForSeconds(2);
        _CloseAppPanel.SetActive(false);
    }
    private void HandleTouchInput()
    {
        _touch = Input.GetTouch(0);

        if(_touch.phase == TouchPhase.Began)
        {
            _touchStartPosition = _camera.ScreenToWorldPoint(_touch.position);

            if (_hitInfo = Physics2D.Raycast(_touchStartPosition, _camera.transform.forward))
            {
                _touchedBlock = _hitInfo.collider.gameObject.GetComponent<NumberBlock>();
            }
        }

        if(_touch.phase == TouchPhase.Ended) {
            if(_touchedBlock == null)
            {
                //Debug.LogError("no block touched");
                return;
            }

            _touchEndPosition = _camera.ScreenToWorldPoint(_touch.position);

            if(IsSwipingHorizontally())
            {
                if (_touchEndPosition.x < _touchStartPosition.x)
                {
                    //Debug.Log(_touchedBlock.name + " moved left");
                    _touchedBlock.HandleBlockMove(MoveDirection.MoveLeft);
                }
                else
                {
                    //Debug.Log(_touchedBlock.name + "moved right");
                    _touchedBlock.HandleBlockMove(MoveDirection.MoveRight);
                }
            } else
            {
                if (_touchEndPosition.y < _touchStartPosition.y)
                {
                    //Debug.Log(_touchedBlock.name + " moved down");
                    _touchedBlock.HandleBlockMove(MoveDirection.MoveDown);
                }
                else
                {
                    //Debug.Log(_touchedBlock.name + "moved up");
                    _touchedBlock.HandleBlockMove(MoveDirection.MoveUp);
                }
            }
            _touchedBlock = null;
        }       
    }

    /// <summary>
    /// Checks whether player swiped horizontally or vertically
    /// </summary>
    /// <returns></returns>
    private bool IsSwipingHorizontally()
    {
        var __yDistance = Mathf.Abs(_touchStartPosition.y - _touchEndPosition.y);
        var __xDistance = Mathf.Abs(_touchStartPosition.x - _touchEndPosition.x);

        if (__yDistance > __xDistance)
        {
            return false;
        }

        return true;
    }

    #region Button actions
    private void Reset()
    {
        foreach(var obj in _NumberBlocks)
        {
            obj.transform.localPosition = obj.initialPosition;
            obj.number = obj.defaultNumber;
            obj.numberText.text = obj.defaultNumber.ToString();
            obj.gameObject.SetActive(true);
        }
    }

    private void SwitchToLevelSelectionMenu()
    {
        Reset();
        InLevelPanel.SetActive(false);
        InitLevelSelectionMenu();
        LevelSelectionPanel.SetActive(true);
    }

    private void Restart()
    {
        levelsClearedOnce = true;

        PlayerPrefs.SetInt("CurrentLevel", 0);

        _Levels[_currentLevel-1].SetActive(false);

        _currentLevel = 0;

        _GameOverPanel.SetActive(false);

        InitNumberBlocks();
        UpdateLevelNumber();

        _Levels[_currentLevel].SetActive(true);
    }

    private void Quit()
    {
        Application.Quit();
    }

    #endregion
    public void CheckGameOver()
    {
        bool __isGameOver = true;

        foreach(var obj in _NumberBlocks)
        {
            if (obj.gameObject.activeSelf) {
                __isGameOver = false;
                break;
            }
        }

        if(__isGameOver)
        {
            OnGameOver();
        }
    }

    private void OnGameOver()
    {
        _Levels[_currentLevel].SetActive(false);
        GlobalVariables.LevelCleared[_currentLevel] = true;

        _currentLevel++;

        GlobalVariables.totalUnlockedLevels = Mathf.Max(GlobalVariables.totalUnlockedLevels, _currentLevel);

        _NumberBlocks.Clear();

        if (_currentLevel != _totalLevels)
        {
            InitNumberBlocks();
            UpdateLevelNumber();          
        } else
        {
            Debug.Log("CONGRATULATIONS");
            _GameOverPanel.SetActive(true);
        }
    }

    public void InitNumberBlocks()
    {
        var NextLevelNumberBlocks = _Levels[_currentLevel].GetComponentsInChildren<NumberBlock>(true);

        //print(levelsClearedOnce);

        foreach (var obj in NextLevelNumberBlocks)
        {
            if (GlobalVariables.LevelCleared[_currentLevel])
            {
                obj.transform.localPosition = obj.initialPosition;
                obj.number = obj.defaultNumber;
                obj.numberText.text = obj.defaultNumber.ToString();
                obj.gameObject.SetActive(true);
            }

            _NumberBlocks.Add(obj);
        }
    }

    private void InitLevelSelectionMenu()
    {
        _Levels[_currentLevel].SetActive(false);

        //var __totalUnlockedLevels = PlayerPrefs.GetInt("CurrentLevel");

        //print(__totalUnlockedLevels);

        for (int i = 0; i < _totalLevels; i++) {
            if(i <= GlobalVariables.totalUnlockedLevels)
            {
                _LevelButtons[i].Level_Button.interactable = true;
                _LevelButtons[i].LevelButtonText.text = (i+1).ToString();
                _LevelButtons[i].LevelButtonImage.color = Color.white;
            }
            else
            {
                _LevelButtons[i].Level_Button.interactable = false;
                _LevelButtons[i].LevelButtonText.text = "";
                _LevelButtons[i].LevelButtonImage.color = Color.grey;
            }
        }
    }
}
