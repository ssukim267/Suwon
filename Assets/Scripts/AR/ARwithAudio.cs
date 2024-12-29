using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class ARwithAudio : MonoBehaviour
{
    // ARTrackedImageManager�� ���� AR �̹��� ���� ����
    public ARTrackedImageManager trackedImageManager;

    // 3D ������Ʈ ����Ʈ
    public List<GameObject> _objectList = new List<GameObject>();

    // ������Ʈ�� ���� ���� �ؽ�Ʈ
    public TextMeshProUGUI objectDescriptionText;

    // ������Ʈ�� ���� ������ ������ ��ųʸ�
    public Dictionary<string, AudioClip> audioDic = new Dictionary<string, AudioClip>();

    // AudioSource ������Ʈ
    private AudioSource audioSource;

    // �̹��� �̸��� Ű��, �ش� �̹����� ������ �� ǥ���� 3D ������Ʈ�� ������ ������ ��ųʸ�
    private Dictionary<string, GameObject> _prefabDic = new Dictionary<string, GameObject>();

    // ���� �ؽ�Ʈ�� �̸� ����
    private Dictionary<string, string> _objectDescriptions = new Dictionary<string, string>()
    {
        {"gyojeon", "�ְ˱����� �Ϻ����� ���Ե� Į���� ���˹����̴�. �� ����� ���� ���� Į�� ����Ͽ� �����ϴ� �������� ������ �ڼ��� ���ϴ� ���� Ư¡�̴�."},
        {"deungpae", "���д� �߱����� ���Ե� Į���� ���˹����̴�. ���п� �Բ� ª�� Į�� �䵵�� ǥâ���� �����Ͽ�, ���� ǥâ�� ������ ���� ���п� Į�� ���� �����ϴ� �������� ��ģ��."},
        {"ssanggeom", "�ְ��� �߱����� ���Ե� Į���� ���˹����̴�. �� ���� Į�� ����ϴ� �⿹�μ� �ϳ��δ� ���� �ٸ� Į�� ��븦 ���ų� ���."},
        {"ssangsudo", "�ּ����� �Ϻ����� ���Ե� Į���� ���˹����̴�. ��� �˹��� �⺻���μ� ���� ��뿡 �ʿ��� �ߵ���, ����, ����, ���, ���˼�, �ȹ�, ���� ������ �̷���� �ִ�."},
        {"nangseon", "������ �߱����� ���Ե� â���� �ܺ������̴�. ������ �볪�� ������ ���� �� ���� 9������ 11���� �޾� ���� ����� ��� ������ �ڹ��� ����ϰ� �ִ�."},
        {"dangpa", "���Ĵ� �߱����� ���Ե� â���� �����̴�. �����̶�� �Ҹ��� ����� �� ���� �������� �� ���� ������ ���� ����� ���� �ִ�."},
        {"woldo", "������ �߱����� ���Ե� Į���� ���˹����̴�. ���� ���·� ��� ���� ������ ����ϰ� �ִ�."},
        {"hyeopdo", "������ �߱����� ���Ե� Į���� ���˹����̴�. ���ó�� ���� ����� �ϰ� ���� �̿��Ͽ� ��� ���� ������ ������ ����ϰ� �ִ�."},
        {"gwonbeop", "�ǹ��� �հ� ���� �̿��Ͽ� ������ �����ϰų� ����ϴ� �Ǽ� �����̴�. ������ ���� �ڰ� ���� ���� ������ �������� �����μ� �ָ����� ġ�� ������ �ݹ��� ����ϰ� �ִ�."},
    };

    // ������Ʈ�� ��� �ִ� �θ� ������Ʈ
    public GameObject ObjectPool;

    // ������Ʈ Ȯ�� �� ���
    private float previousTouchDistance = 0;
    private Vector3 initialScale;

    // ������Ʈ ȸ��
    private float previousTouchPositionX = 0f;
    private float startRotationY = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // ������Ʈ ����Ʈ�� �ִ� 3D ������Ʈ�� �̸��� �Բ� ��ųʸ��� �߰�
        foreach (GameObject obj in _objectList)
        {
            string tName = obj.name;
            _prefabDic.Add(tName, obj); // ������Ʈ �̸��� Ű�� ��ųʸ��� ����
        }
    }

    private void OnEnable()
    {
        // �̹����� �����ǰų� ������Ʈ�� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯 ���
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        // ������Ʈ�� ��Ȱ��ȭ�Ǹ� �̺�Ʈ �ڵ鷯 ����
        trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            ChangeScale();
        }
        else if (Input.touchCount == 1)
        {
            OnTouchRotate();
        }
    }

    void ChangeScale()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            previousTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            initialScale = ObjectPool.transform.localScale;
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float currentTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            if (Mathf.Approximately(previousTouchDistance, 0)) return;

            float scaleFactor = currentTouchDistance / previousTouchDistance;
            ObjectPool.transform.localScale = initialScale * scaleFactor;
        }
    }

    void OnTouchRotate()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPositionX = touch.position.x;
                startRotationY = ObjectPool.transform.eulerAngles.y;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float deltaPositionX = touch.position.x - previousTouchPositionX;
                float rotationFactor = deltaPositionX * 0.2f;
                float newRotationY = startRotationY + rotationFactor;
                ObjectPool.transform.rotation = Quaternion.Euler(0, newRotationY, 0);
            }
        }
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            if (trackedImage != null)
            {
                UpdateImage(trackedImage);
                PlayAudio(trackedImage.referenceImage.name); // �̹����� �ش��ϴ� ���� ���
            }
            else
            {
                Debug.LogWarning("Null trackedImage in added event.");
            }
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage != null)
            {
                UpdateImage(trackedImage);
                PlayAudio(trackedImage.referenceImage.name);
            }
            else
            {
                Debug.LogWarning("Null trackedImage in updated event.");
            }
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            if (trackedImage != null)
            {
                DisableImage(trackedImage);
            }
            else
            {
                Debug.LogWarning("Null trackedImage in removed event.");
            }
        }
    }

    // �̹����� �ش��ϴ� ������Ʈ�� ��ġ�� ȸ���� ������Ʈ�ϰ� ������Ʈ Ȱ��ȭ
    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (_prefabDic.ContainsKey(name))
        {
            GameObject tObj = _prefabDic[name];
            tObj.transform.position = trackedImage.transform.position;
            tObj.SetActive(true);

            if (_objectDescriptions.ContainsKey(name))
            {
                string description = _objectDescriptions[name];
                objectDescriptionText.text = description;
                objectDescriptionText.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("No prefab found for tracked image: " + name);
        }
    }



    // �̹��� ���� ���� �� �ش� ������Ʈ ��Ȱ��ȭ
    private void DisableImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (_prefabDic.ContainsKey(name))
        {
            GameObject tObj = _prefabDic[name];
            tObj.SetActive(false);

            // �̹��� ������ ������ ������ ���� ��� ���� �ʱ�ȭ
            isAudioPlayed = false;
        }
        else
        {
            Debug.LogWarning("No prefab found for tracked image to disable: " + name);
        }
    }


    // �̹����� �ش��ϴ� ���� ���
    public List<AudioClip> audioList = new List<AudioClip>(); // ����� Ŭ�� ����Ʈ

    private bool isAudioPlayed = false; // ������ �̹� ����Ǿ����� ���θ� �����ϴ� ����

    // �̹����� �������� ���� ���
    private void PlayAudio(string imageName)
    {
        // ����Ʈ���� ���� ã�� (�̹��� �̸��� �������� ��Ī)
        int audioIndex = GetAudioIndexForImage(imageName);

        if (audioIndex >= 0 && audioIndex < audioList.Count)
        {
            AudioClip clip = audioList[audioIndex];

            // ������ ��� ������ Ȯ��
            if (audioSource.isPlaying)
            {
                Debug.Log("������� ������Դϴ�");
                return;  // ������ �̹� ��� ���̸� ����� �����ϰ� ��ȯ
            }

            audioSource.clip = clip;
            audioSource.Play();
            isAudioPlayed = true;  // ������ ����Ǿ����� ǥ��
        }
        else
        {
            Debug.LogWarning("�� �̹����� �ش��ϴ� ������ �����ϴ�: " + imageName);
        }
    }

    private int GetAudioIndexForImage(string imageName)
    {
        // �̹��� �̸��� �´� ���� �ε����� ��ȯ�ϴ� ���� ����
        Dictionary<string, int> imageAudioIndexMap = new Dictionary<string, int>()
    {
        { "gyojeon", 0 },
        { "deungpae", 1 },
        { "ssanggeom", 2 },
        { "ssangsudo", 3 },
        { "nangseon", 4 },
        { "dangpa", 5 },
        { "woldo", 6 },
        { "hyeopdo", 7 },
        { "gwonbeop", 8 }
        // �߰������� �ʿ��� �̹����� ���� �ε��� ���� �߰� ����
    };

        // �̹��� �̸��� ��ųʸ��� �ִ��� Ȯ���ϰ�, ������ �ش� �ε����� ��ȯ
        if (imageAudioIndexMap.ContainsKey(imageName))
        {
            return imageAudioIndexMap[imageName];
        }
        else
        {
            return -1;  // �ش��ϴ� ������ ������ -1�� ��ȯ
        }
    }

}
