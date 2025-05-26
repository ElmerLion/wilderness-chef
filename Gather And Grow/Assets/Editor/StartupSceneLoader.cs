using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;


[InitializeOnLoad]
public static class StartupSceneLoader {

    static StartupSceneLoader() {
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange state) {
        if (state == PlayModeStateChange.ExitingEditMode) {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();


        }

        if (state == PlayModeStateChange.EnteredPlayMode) {
            if (EditorSceneManager.GetActiveScene().buildIndex != 0) {
                EditorSceneManager.LoadScene(0);
            }
        }
    }
}
