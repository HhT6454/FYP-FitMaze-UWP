using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : BaseMenuManager
{
    protected override string GetMenuSceneName()
    {
        return "PauseMenu";
    }

    public void OnResumeButtonPressed()
    {
        if (levelManager != null)
        {
            levelManager.ResumeGame();
        }
        else
        {
            SceneManager.UnloadSceneAsync("PauseMenu"); // Fallback if LevelManager is not found
            Time.timeScale = 1f; // Resume time
            BLEManager.Instance?.bleConnect?.UpdateSensorStateOnBLE("start");
        }
    }
}
