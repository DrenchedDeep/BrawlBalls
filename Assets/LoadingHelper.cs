using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingHelper : MonoBehaviour
{
    private static LoadingHelper Instance { get; set; }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.SetActive(false);
    }

    public static void Activate()
    {
        Instance.gameObject.SetActive(true);
    }

    public static void Deactivate()
    {
        Instance.gameObject.SetActive(false);
    }

}
