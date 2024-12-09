using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void OnclickAI()
    {
        SceneManager.LoadScene(2);
    }

    public void OnclickCancel()
    {
        SceneManager.LoadScene(1);
    }
}
