using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void OnclickCancel()
    {
        SceneManager.LoadScene(0);
    }

    public void OnclickTmap()
    {
        SceneManager.LoadScene(1);
    }

    public void OnclickAI()
    {
        SceneManager.LoadScene(2);
    }

    public void OnclickAR()
    {
        SceneManager.LoadScene(3);
    }
}
