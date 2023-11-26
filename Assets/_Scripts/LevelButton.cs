using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI LevelButtonText;
    public Image LevelButtonImage;
    public Button Level_Button;

    private void OnEnable()
    {
        Level_Button.onClick.AddListener(OnLevelClicked);
    }

    private void OnDisable()
    {
        Level_Button.onClick.RemoveListener(OnLevelClicked);
    }

    private void OnLevelClicked()
    {
        GameManager.isLevelSelected = true;

        GameManager.instance.LevelSelectionPanel.SetActive(false);
        GameManager.instance._currentLevel = int.Parse(LevelButtonText.text)-1;

        GameManager.instance._NumberBlocks.Clear();

        GameManager.instance.InitNumberBlocks();
        GameManager.instance.UpdateLevelNumber();
        GameManager.instance.InLevelPanel.SetActive(true);
    }
}
