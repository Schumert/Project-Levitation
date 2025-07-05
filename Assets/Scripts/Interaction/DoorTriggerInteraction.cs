using UnityEngine;

public class DoorTriggerInteraction : TriggerInteractionBase
{

    public enum DoorToSpawnAt
    {
        None,
        One,
        Two,
        Three,
        Four,
    }


    [Header("Spawn TO")]
    [SerializeField] private SceneField[] scenesToLoad;
    [SerializeField] private DoorToSpawnAt DoorToSpawnTo;


    [Space(10f)]
    [Header("THIS Door")]
    public DoorToSpawnAt CurrentDoorPosition;


    public override void Interact()
    {
        SceneSwapManager.SwapSceneFromDoorUse(scenesToLoad, DoorToSpawnTo);
    }
}
