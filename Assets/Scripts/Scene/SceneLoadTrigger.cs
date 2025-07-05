using System.Linq;
using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private SceneField[] _sceneToLoad;
    [SerializeField] private SceneField[] _sceneToUnload;

    private GameObject player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {

            LoadScenes();
            UnloadScenes();
        }
    }

    private void LoadScenes()
    {
        for (int i = 0; i < _sceneToLoad.Length; i++)
        {
            bool isSceneLoaded = false;
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == _sceneToLoad[i].SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }

            }

            if (!isSceneLoaded)
            {
                SceneManager.LoadSceneAsync(_sceneToLoad[i], LoadSceneMode.Additive);
            }
        }
    }

    private void UnloadScenes()
    {

        for (int i = 0; i < _sceneToUnload.Length; i++)
        {
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == _sceneToUnload[i].SceneName)
                {
                    SceneManager.UnloadSceneAsync(_sceneToUnload[i]);
                }

            }
        }
    }
}
