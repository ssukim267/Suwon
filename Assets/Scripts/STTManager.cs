using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using OpenAI;

public class STTManager : MonoBehaviour
{
    private string apiKey = "AIzaSyDBgjyaZMEztMaU6qedbWJRaYNLMougoK0";
    private string apiUrl = "https://speech.googleapis.com/v1/speech:recognize?key=";
    private AudioSource audioSource;
    public Button startRecordingButton;
    //public Button stopRecordingButton;

    private AudioClip recordedClip;
    private bool isRecording = false;

    private int CHUNK_SIZE = 15; 
    private float maxSilenceTime = 10f; 

    public InputField resultInputField;
    public ChatGPT chatGPT;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        startRecordingButton.onClick.AddListener(StartRecording);
    }

    // 녹음 시작
    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 120, 44100); 
            StartCoroutine(RecordingTimeout());
            StartCoroutine(CheckSilence()); // 공백 감지 시작
            Debug.Log("녹음 시작");
        }
    }

    // 녹음 시간 제한
    private IEnumerator RecordingTimeout()
    {
        yield return new WaitForSeconds(120);
        if (isRecording)
        {
            StopRecording();
        }
    }

    // 공백 감지
    private IEnumerator CheckSilence()
    {
        float[] samples = new float[300];
        float silenceDuration = 0;

        while (isRecording)
        {
            int micPosition = Microphone.GetPosition(null) - samples.Length;
            if (micPosition < 0)
            {
                yield return null;  
                continue;
            }

            recordedClip.GetData(samples, micPosition);

            bool hasSound = false;
            foreach (float sample in samples)
            {
                if (Mathf.Abs(sample) > 0.1f) // 음량 체크
                {
                    hasSound = true;
                    break;
                }
            }

            if (!hasSound)
            {
                silenceDuration += Time.deltaTime;

                if (silenceDuration >= maxSilenceTime) // 공백 시간이 초과
                {
                    StopRecording();
                    yield break;
                }
            }
            else
            {
                silenceDuration = 0; // 소리가 감지되면 공백 시간 리셋
            }

            yield return null;
        }
    }

    // 녹음 종료
    public void StopRecording()
    {
        if (isRecording)
        {
            if (Microphone.IsRecording(null))
            {
                Microphone.End(null);
            }

            isRecording = false;

            if (recordedClip == null || recordedClip.samples == 0)
            {
                Debug.LogError("녹음된 데이터 없음");
                return;
            }

            Debug.Log("녹음 종료");
            StartCoroutine(ProcessAudioChunks());
        }
    }

    // 청크 처리
    private IEnumerator ProcessAudioChunks()
    {
        int sampleRate = 44100;
        int chunkSamples = CHUNK_SIZE * sampleRate;

        for (int startSample = 0; startSample < recordedClip.samples; startSample += chunkSamples)
        {
            int endSample = Mathf.Min(startSample + chunkSamples, recordedClip.samples);
            float[] chunkData = new float[endSample - startSample];
            recordedClip.GetData(chunkData, startSample);

            AudioClip chunkClip = AudioClip.Create("chunk", chunkData.Length, recordedClip.channels, sampleRate, false);
            chunkClip.SetData(chunkData, 0);

            byte[] audioData = WavUtility.FromAudioClip(chunkClip);
            yield return SendSTTRequest(audioData);
        }
    }

    // API 요청
    private IEnumerator SendSTTRequest(byte[] audioData)
    {
        string jsonRequest = "{\"config\":{\"encoding\":\"LINEAR16\",\"sampleRateHertz\":44100,\"languageCode\":\"ko-KR\"},\"audio\":{\"content\":\"" + System.Convert.ToBase64String(audioData) + "\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

        using (UnityWebRequest www = new UnityWebRequest(apiUrl + apiKey, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API 오류: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                var response = JsonUtility.FromJson<STTResponse>(jsonResponse);

                if (response.results.Length > 0)
                {

                    string transcript = response.results[0].alternatives[0].transcript;
                    //resultInputField.text += transcript + " ";

                    //전체 텍스트 전달
                    string finalText = string.Join(" ", transcript); 
        
                    resultInputField.text = finalText;
                    chatGPT.SendReply();

                }
                else
                {
                    Debug.Log("텍스트 없음");
                }
            }
        }
    }

    // STT 응답 클래스 정의
    [System.Serializable]
    public class STTResponse
    {
        public Result[] results;

        [System.Serializable]
        public class Result
        {
            public Alternative[] alternatives;

            [System.Serializable]
            public class Alternative
            {
                public string transcript;
            }
        }
    }
}
