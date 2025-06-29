using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectButtons : MonoBehaviour
{
    [SerializeField] private Button defaultLevelButton;
    [SerializeField] private Button customLevelButton;

    void Awake()
    {
        if (defaultLevelButton != null)
            defaultLevelButton.onClick.AddListener(LoadDefaultLevelSelect);
        else
            Debug.LogWarning("Default level button not assigned");

        if (customLevelButton != null)
            customLevelButton.onClick.AddListener(LoadCustomLevelSelect);
        else
            Debug.LogWarning("Custom level button not assigned");
    }
    public void LoadDefaultLevelSelect()
    {
        SceneManager.LoadSceneAsync("DefaultLevelSelect");
    }

    public void LoadCustomLevelSelect()
    {
        SceneManager.LoadSceneAsync("CustomLevelSelect");
    }
}
