using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Main menu Play button: loads the battle scene asynchronously, so the menu keeps
// rendering (showing "Loading...") instead of freezing while the scene loads.
public class StartGame : MonoBehaviour
{
    [SerializeField] string sceneName = "SampleScene";
    [SerializeField] Button playButton;
    [SerializeField] TMP_Text playLabel;

    bool loading;

    // Wired to the Play button. Button handler, so async void is fine.
    public async void OnPlayPressed()
    {
        if (loading) return;   // ignore double taps

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[menu] '{sceneName}' is not in the Build Profiles scene list.");
            return;
        }

        loading = true;
        playButton.interactable = false;
        if (playLabel != null) playLabel.text = "Loading...";
        Telemetry.Log("menu_play", ("scene", sceneName));

        // This menu is unloaded when the load finishes, so nothing runs after the await.
        await SceneManager.LoadSceneAsync(sceneName);
    }
}
