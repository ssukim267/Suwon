using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;


public class ARTrackedImg : MonoBehaviour
{
    // ARTrackedImageManager�� ���� AR �̹��� ���� ����
    public ARTrackedImageManager trackedImageManager;

    // 3D ������Ʈ ����Ʈ
    public List<GameObject> _objectList = new List<GameObject>();

    public TextMeshProUGUI objectDescriptionText; // ������Ʈ ������ ǥ���� UI �ؽ�Ʈ

    public NaverTTS tts; // NaverTTS ��ũ��Ʈ�� ����

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

    // ������Ʈ Ȯ�� �� ���
    private float previousTouchDistance = 0; // ���� �հ��� �Ÿ�
    private Vector3 initialScale;
    public GameObject ObjectPool; // ������Ʈ�� ��� �ִ� �θ� ������Ʈ

    // ������Ʈ ȸ��
    //public Transform chr; 
    private float previousTouchPositionX = 0f; // ���� ��ġ ��ġ
    private float startRotationY = 0f; // �ʱ� ȸ�� ��

    void Awake()
    {
        // ������Ʈ ����Ʈ�� �ִ� 3D ������Ʈ�� �̸��� �Բ� ��ųʸ��� �߰�
        foreach (GameObject obj in _objectList)
        {
            string tName = obj.name;  // �� ������Ʈ�� �̸��� ������
            _prefabDic.Add(tName, obj);  // ������Ʈ �̸��� Ű�� ��ųʸ��� ���� (������ �̹����� ������ �غ�)
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

    private float currentRotationY = 0f;


    void Update()
    {
        if (Input.touchCount == 2) // ũ�� ����
        {
            ChangeScale();
        }
        else if (Input.touchCount == 1) // ȸ��
        {
            OnTouchRotate();
        }
    }

    void ChangeScale()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // ��ġ ���� �� �ʱ� �Ÿ�, ObjectPool�� �ʱ� ũ�� ����
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            previousTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            initialScale = ObjectPool.transform.localScale;
        }
        // ��ġ �̵� �� ObjectPool�� ũ�� ����
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float currentTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            if (Mathf.Approximately(previousTouchDistance, 0)) return;

            // ���� �Ÿ��� �ʱ� �Ÿ��� ������ ���� ũ�� ����
            float scaleFactor = currentTouchDistance / previousTouchDistance;
            ObjectPool.transform.localScale = initialScale * scaleFactor;  // ObjectPool�� ������ ����
        }
        
    }



    void OnTouchRotate()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // ��ġ ���� �� �ʱ� ȸ�� �� ����
            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPositionX = touch.position.x;
                startRotationY = ObjectPool.transform.eulerAngles.y; // �ʱ� ȸ�� ��
            }

            // ��ġ �̵� �� ȸ�� ����
            else if (touch.phase == TouchPhase.Moved)
            {
                // �հ��� �̵��� ���ϱ�
                float deltaPositionX = touch.position.x - previousTouchPositionX;

                // �̵� �Ÿ��� ���� ȸ�� ������ ���
                float rotationFactor = deltaPositionX * 0.2f; // 0.2�� ȸ�� �ӵ�

                // ȸ���� ������Ʈ
                float newRotationY = startRotationY + rotationFactor;
                ObjectPool.transform.rotation = Quaternion.Euler(0, newRotationY, 0); // Y�� ���� ȸ��
            }
        }
    }


    // �̹����� ���� �νĵǾ��ų� ������Ʈ�� ��쿡 ȣ���
    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // ���� �߰��� �̹��� ó��
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);  // �̹����� �ش��ϴ� ������Ʈ�� ��ġ
        }

        // ������Ʈ�� �̹��� ó�� (���� �̹����� �����̰ų� ���°� ����� ���)
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);  // �̹����� �����ϴ� ������Ʈ�� ��ġ �� ȸ���� ������Ʈ
        }

        // ������ ������ �̹��� ó�� (�̹����� ������ ���� ��)
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            DisableImage(trackedImage);  // ������Ʈ ��Ȱ��ȭ
        }
    }

    // ������ �̹����� �����ϴ� ������Ʈ�� ��ġ�� ȸ���� ������Ʈ�ϰ�, ������Ʈ�� Ȱ��ȭ
    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;  // ������ �̹����� �̸��� ������
        GameObject tObj = _prefabDic[name];  // �ش� �̸��� ���ε� ������Ʈ�� ��ųʸ����� ������
        tObj.transform.position = trackedImage.transform.position;  // �̹����� �ִ� ��ġ�� ������Ʈ ��ġ�� ����
        tObj.SetActive(true);  // ������Ʈ�� Ȱ��ȭ�Ͽ� ���� ǥ��

        // ������Ʈ�� ���� ������ UI�� ǥ��
        if (_objectDescriptions.ContainsKey(name))
        {
            string description = _objectDescriptions[name];
            objectDescriptionText.text = description;
            objectDescriptionText.gameObject.SetActive(true);
            tts.text = description; // ���� �ؽ�Ʈ�� TTS�� ����
        }
    }

    // ������ ������ �̹����� �����ϴ� ������Ʈ�� ��Ȱ��ȭ
    private void DisableImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;  // ������ �̹����� �̸��� ������
        if (_prefabDic.ContainsKey(name))
        {
            GameObject tObj = _prefabDic[name];  // �ش� �̸��� ���ε� ������Ʈ�� ��ųʸ����� ������
            tObj.SetActive(false);  // ������Ʈ�� ��Ȱ��ȭ�Ͽ� ������ ����
        }

        // ������Ʈ ���� UI �����
        objectDescriptionText.gameObject.SetActive(false);
    }
}
