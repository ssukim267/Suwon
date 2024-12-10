using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    public Text storyText;

    private string[] content = new string[]
    {
        "202X년 화성행궁에 도착하였습니다. \nAI요원 설아의 도움을 받아\n판테온의 만행을 저지하세요.",
         "2번 내용",
         "3번 내용",
         "4번 내용",
    };

    private int index = 0;
        
  
    public void NextContent()
    {
        if (index < content.Length - 1) 
        {
            index++; 
            storyText.text = content[index]; 
        }
    }

    public void PreviousContent()
    {
        if (index > 0)
        {
            index--;
            storyText.text = content[index];
        }
    }
}
