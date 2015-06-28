using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// Based on http://www.daikonforge.com/dfgui/save-on-run/
/// </summary>
[InitializeOnLoad]
public class SaveOnPlay : MonoBehaviour
{
    static SaveOnPlay()
    {
        EditorApplication.playmodeStateChanged = () =>
        {
            if( EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying )
            {
                Debug.Log( "Auto-Saving scene before entering Play mode: " + EditorApplication.currentScene );
                
                EditorApplication.SaveScene();
                EditorApplication.SaveAssets();
            }
        };
    }
}
