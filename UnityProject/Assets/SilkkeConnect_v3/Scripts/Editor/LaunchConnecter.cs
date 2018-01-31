using UnityEditor;
using UnityEditor.SceneManagement;

public class LaunchConnecter {

	[MenuItem("Silkke/Launch From Connecter")]
	static void Launch()
	{
        if (EditorApplication.isPlaying == false)
        {
            EditorSceneManager.OpenScene("Assets/SilkkeConnect_v3/Scenes/Login.unity");
            EditorApplication.isPlaying = true;
        }
    }
}
