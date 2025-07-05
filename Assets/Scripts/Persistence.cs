using Unity.VisualScripting;
using UnityEngine;

public class Persistence : MonoBehaviour
{
    public static Persistence instance;

    [SerializeField] private GameObject player;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    static public void BringPlayer()
    {
        instance.player.transform.SetParent(instance.transform);


        instance.InitializePlayer();
    }

    private void InitializePlayer()
    {
        player = GameObject.FindWithTag("Player");
    }
}
