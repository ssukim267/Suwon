using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;


public class TTSManager : MonoBehaviour
{
    private string apiUrl = "https://texttospeech.googleapis.com/v1/text:synthesize?key=";
    private string apiKey = "AIzaSyDBgjyaZMEztMaU6qedbWJRaYNLMougoK0";


    private AudioSource audioSource;

    public string text;
    private string previousText = null;

    void Start()
    {
        // AudioSource를 컴포넌트에 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        //text값이 새로 들어왔는지? 
        if (!string.IsNullOrEmpty(text) && text != previousText)
        {
            OnNewText();
            previousText = text;  // 이전 값 업데이트
            //Debug.Log(text);
        }
    }

   
    void OnNewText()
    {

        string inputText = text;


        GoogleCloudTextToSpeechRequest request = new GoogleCloudTextToSpeechRequest()
        {
            input = new SynthesisInput() { text = inputText },
            voice = new VoiceSelectionParams()
            {
                languageCode = "ko-KR",
                name = "ko-KR-Standard-A",
                ssmlGender = "FEMALE"
            },
            audioConfig = new AudioConfig() { audioEncoding = "MP3" }
        };

        ConvertTextToSpeech(request);
    }


    public void ConvertTextToSpeech(GoogleCloudTextToSpeechRequest request)
    {

        string jsonRequest = JsonUtility.ToJson(request);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);


        StartCoroutine(SendTTSRequest(bodyRaw));
    }

    // API 요청
    IEnumerator SendTTSRequest(byte[] bodyRaw)
    {
        using (UnityWebRequest www = new UnityWebRequest(apiUrl + apiKey, "POST"))
        {

            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();


            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {

                // API 응답 JSON으로 변환
                string jsonResponse = www.downloadHandler.text;
                TTSResponse response = JsonUtility.FromJson<TTSResponse>(jsonResponse);

                // 오디오 디코딩
                byte[] audioBytes = System.Convert.FromBase64String(response.audioContent);

                string filePath = Path.Combine(Application.persistentDataPath, "ttsAudio.mp3");
                File.WriteAllBytes(filePath, audioBytes);
                Debug.Log("Audio file saved at: " + filePath);

                StartCoroutine(PlayAudio(filePath));
            }
        }
    }

    // 오디오 파일 재생
    IEnumerator PlayAudio(string filePath)
    {

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();
                Debug.Log("Audio playing...");
            }
        }
    }
}


[System.Serializable]
public class TTSResponse
{
    public string audioContent;
}

[System.Serializable]
public class SynthesisInput
{
    public string text;
}

[System.Serializable]
public class VoiceSelectionParams
{
    public string languageCode;
    public string name;
    public string ssmlGender;
}

[System.Serializable]
public class AudioConfig
{
    public string audioEncoding;
}

[System.Serializable]
public class GoogleCloudTextToSpeechRequest
{
    public SynthesisInput input;
    public VoiceSelectionParams voice;
    public AudioConfig audioConfig;
}