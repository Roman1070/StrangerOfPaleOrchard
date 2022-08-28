using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorToolsTab
{
    [MenuItem("Tools/ForceStart")]
    public static void ForceStart()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.EnterPlaymode();
    }

    [MenuItem("Tools/GameScene")]
    public static void OpenGameScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
    }
}
