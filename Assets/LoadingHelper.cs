using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingHelper : MonoBehaviour
{
    private static LoadingHelper _instance;
    
       
    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInit()
    {
        _instance = null;
    }

    private void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        gameObject.SetActive(false);
    }

    public static void Activate()
    {
        _instance.gameObject.SetActive(true);
    }

    public static void Deactivate()
    {
        _instance.gameObject.SetActive(false);
    }

}
