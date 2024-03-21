using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    
    [SerializeField] string targetSceneName;

    // Function to change the scene
    public void ChangeSceneFunction() => SceneManager.LoadScene(targetSceneName);

}
