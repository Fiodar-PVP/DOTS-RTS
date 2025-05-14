using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    private void Start()
    {
        DOTSEventManager.Instance.OnBuildingHQDead += DOTSEventManager_OnBuildingHQDead;

        Hide();    
    }

    private void DOTSEventManager_OnBuildingHQDead(object sender, System.EventArgs e)
    {
        Show();

        Time.timeScale = 0f;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
