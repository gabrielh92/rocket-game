using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Canvas levelCanvas;
    [SerializeField] GameObject levelHolder;
    [SerializeField] GameObject levelIcon;
    [SerializeField] GameObject previousButton;
    [SerializeField] GameObject nextButton;

    private class Level {
        public bool isAvailable;
        int id;
        Button levelSelect;
        int deaths;

        public Level(int _id, bool _isAvailable) {
            id = _id;
            isAvailable = _isAvailable;

            foreach (var _button in GameObject.FindObjectsOfType<Button>()) {
                if(_button.name == id.ToString()) {
                    levelSelect = _button;
                    UpdateInteractability();
                    _button.GetComponentInChildren<TextMeshProUGUI>().SetText(_button.name);
                    _button.onClick.AddListener(delegate { 
                        GameManager.instance.LevelButtonPressed(Int32.Parse(_button.name));
                    });
                    break;
                }
            }

            deaths = 0;
        }

        public void Died() {
            deaths++;
        }

        public int GetDeaths() {
            return deaths;
        }

        public void UpdateInteractability() {
            levelSelect.interactable = isAvailable;
        }

        public void MakeLevelAvailable() {
            isAvailable = true;
            UpdateInteractability();
        }
    }

    Dictionary<int, Level> levels;
    Rect panelDimensions, iconDimensions;
    int totalLevels, currentPanelID, currentLevel, amountPerPage;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    private void OnEnable() {
        totalLevels = SceneManager.sceneCountInBuildSettings - 1;
        currentLevel = 1;

        // Generate Buttons in UI
        panelDimensions = levelHolder.GetComponent<RectTransform>().rect;
        iconDimensions = levelIcon.GetComponent<RectTransform>().rect;

        int _maxIconsInRow = Mathf.FloorToInt(panelDimensions.width / iconDimensions.width);
        int _maxIconsInCol = Mathf.FloorToInt(panelDimensions.height / iconDimensions.height);

        amountPerPage = _maxIconsInRow * _maxIconsInCol;
        int _totalPages = Mathf.CeilToInt((float)totalLevels / amountPerPage);
        LoadPanels(_totalPages, amountPerPage);

        // Create Levels Dictionary
        levels = new Dictionary<int, Level>();
        for (int i = 1; i <= totalLevels; i++) {
            levels.Add(i, new Level(i, (i == 1)));
        }

        SceneManager.sceneLoaded += OnSceneEnabled;
    }

    private void OnSceneEnabled(Scene _scene, LoadSceneMode _mode) {
        Rocket _rocket = GameObject.FindGameObjectWithTag("Player").GetComponent<Rocket>();
        if(_scene.buildIndex == 0) {
            levelCanvas.enabled = true;
            _rocket.SetCollision(false);
            for(int i = 1 ; i <= totalLevels ; i++) {
                levels[i].UpdateInteractability();
            }
            SetActivePanelByCurrentLevel();
        } else {
            _rocket.SetCollision(true);
            levelCanvas.enabled = false;
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Q)) {
            SceneManager.LoadScene(0);
        }
    }

    private void LoadPanels(int _number, int _iconsPerPage) {
        GameObject _panelClone = Instantiate(levelHolder) as GameObject;
        int _remainingLevelIcons = totalLevels;
        
        for(int i = 0 ; i < _number ; i++) {
            GameObject _panel = Instantiate(_panelClone) as GameObject;
            _panel.transform.SetParent(levelCanvas.transform, false);
            _panel.transform.SetParent(levelHolder.transform);
            _panel.name = $"Level Page - {i}";
            SetupGrid(_panel);

            int _numberOfIcons = (i == _number-1) ? _remainingLevelIcons : _iconsPerPage;
            LoadIcons(_numberOfIcons, _panel, i * _iconsPerPage);
            _remainingLevelIcons -= _numberOfIcons;
        }
        
        Destroy(_panelClone);
    }

    private void SetupGrid(GameObject _panel) {
        GridLayoutGroup _grid = _panel.AddComponent<GridLayoutGroup>();
        _grid.cellSize = new Vector2(iconDimensions.width, iconDimensions.height);
        _grid.childAlignment = TextAnchor.MiddleCenter;

    }

    private void LoadIcons(int _number, GameObject _parent, int _offset) {
        for(int i = 1 ; i <= _number ; i++) {
            GameObject _icon = Instantiate(levelIcon) as GameObject;
            _icon.transform.SetParent(levelCanvas.transform, false);
            _icon.transform.SetParent(_parent.transform);
            _icon.name = (i + _offset).ToString();
        }
    }

    private void SetActivePanelById(int _id) {
        for(int i = 0 ; i < levelHolder.transform.childCount ; i++) {
            var _child = levelHolder.transform.Find($"Level Page - {i}");
            _child.gameObject.SetActive(i == _id);
        }
        previousButton.GetComponent<Button>().interactable = !(_id == 0);
        nextButton.GetComponent<Button>().interactable = !(_id == levelHolder.transform.childCount - 1);
        currentPanelID = _id;
    }

    private void SetActivePanelByCurrentLevel() {
        int _panelID = Mathf.FloorToInt(currentLevel / amountPerPage);
        SetActivePanelById(_panelID);
    }

    public void SetActivePanelByButton(bool _next) {
        int _nextPanelID = Mathf.FloorToInt(currentPanelID) + ((_next) ? 1 : -1);
        SetActivePanelById(_nextPanelID);
    }

    public void WonLevel() {
        levels[currentLevel].MakeLevelAvailable();
    }

    public void LoadNextLevel() {
        currentLevel++;
        if(currentLevel > totalLevels) {
            currentLevel = 0; // TODO win screen
        } else {
            levels[currentLevel].MakeLevelAvailable();
        }
        LoadCurrentLevel();
    }

    public void LoadCurrentLevel() {
        SceneManager.LoadScene(currentLevel);
    }

    public void LevelButtonPressed(int _id) {
        currentLevel = _id;
        LoadCurrentLevel();
    }

    public void ExitGameButton() {
        Application.Quit();
    }

    public void AddDeath() {
        GameManager.instance.levels[currentLevel].Died();
    }

    public int GetDeaths(int _levelBuildIndex) {
        int _deathCount = 0;

        if(_levelBuildIndex == 0) {
            foreach (var _level in levels.Values) {
                _deathCount += _level.GetDeaths();
            }
        } else {
            _deathCount = GameManager.instance.levels[_levelBuildIndex].GetDeaths();
        }

        return _deathCount;
    }
}
