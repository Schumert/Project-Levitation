using UnityEngine;
using UnityEngine.Assertions.Must;

public class SceneLight : MonoBehaviour
{

    void Start()
    {
        GetComponent<Light>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
