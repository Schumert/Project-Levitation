using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
    public static SceneSwapManager instance;

    public static bool loadFromDoor;

    [SerializeField] private GameObject player;
    private Collider playerColl;
    private Collider doorColl;
    private Vector3 playerSpawnPosition;

    private DoorTriggerInteraction.DoorToSpawnAt doorToSpawnTo;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        playerColl = player.GetComponent<Collider>();

    }



    public static void SwapSceneFromDoorUse(SceneField[] myScenes, DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt)
    {
        loadFromDoor = true;
        instance.StartCoroutine(instance.FadeOutThenChangeScene(myScenes, doorToSpawnAt));
    }

    private IEnumerator FadeOutThenChangeScene(SceneField[] myScenes, DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt = DoorTriggerInteraction.DoorToSpawnAt.None)
    {
        InputManager.DeactivatePlayerControls();

        player.transform.SetParent(null);
        DontDestroyOnLoad(player);

        SceneFadeManager.instance.StartFadeOut();

        while (SceneFadeManager.instance.isFadingOut)
        {
            yield return null;
        }

        doorToSpawnTo = doorToSpawnAt;

        // 🔹 İlk sahne Single (tüm sahneleri temizleyip yükler)
        AsyncOperation firstLoadOp = SceneManager.LoadSceneAsync(myScenes[0], LoadSceneMode.Single);
        while (!firstLoadOp.isDone)
        {
            yield return null;
        }

        // 🔹 Diğer sahneleri varsa Additive olarak sırayla yükle
        if (myScenes.Length > 1)
        {
            for (int i = 1; i < myScenes.Length; i++)
            {
                AsyncOperation addOp = SceneManager.LoadSceneAsync(myScenes[i], LoadSceneMode.Additive);
                while (!addOp.isDone)
                {
                    yield return null;
                }
            }
        }

        // Her şey yüklendiyse ilerle
        OnAllScenesLoaded();
    }



    private IEnumerator ActivatePlayerControlsAfterFadeIn()
    {
        while (SceneFadeManager.instance.isFadingIn)
        {
            yield return null;
        }

        InputManager.ActivatePlayerControls();

    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        SceneFadeManager.instance.StartFadeIn();
        if (loadFromDoor)
        {
            StartCoroutine(ActivatePlayerControlsAfterFadeIn());
            FindDoor(doorToSpawnTo);
            player.GetComponent<Rigidbody>().MovePosition(playerSpawnPosition);
            //print(player.transform.position);
            loadFromDoor = false;
        }
    }

    private void FindDoor(DoorTriggerInteraction.DoorToSpawnAt doorSpawnNumber)
    {
        DoorTriggerInteraction[] doors = FindObjectsOfType<DoorTriggerInteraction>();

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].CurrentDoorPosition == doorSpawnNumber)
            {
                doorColl = doors[i].gameObject.GetComponent<Collider>();

                CalculateSpawnPosition();
                return;
            }
        }
    }

    private void CalculateSpawnPosition()
    {
        float colliderHeight = playerColl.bounds.extents.y;
        playerSpawnPosition = new Vector3(doorColl.transform.position.x + 2, doorColl.transform.position.y, 0f);
        //print(playerSpawnPosition);
    }

    private void OnAllScenesLoaded()
    {
        SceneFadeManager.instance.StartFadeIn();

        if (loadFromDoor)
        {
            StartCoroutine(ActivatePlayerControlsAfterFadeIn());
            FindDoor(doorToSpawnTo);
            player.GetComponent<Rigidbody>().MovePosition(playerSpawnPosition);
            loadFromDoor = false;
        }
    }

}
