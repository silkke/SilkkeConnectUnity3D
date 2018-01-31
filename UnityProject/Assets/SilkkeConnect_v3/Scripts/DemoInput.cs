using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0);
    }
}
