using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class ARwithAudio : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    // 3D ������Ʈ ����Ʈ
    public List<GameObject> _objectList = new List<GameObject>();

    // �̹��� �̸��� Ű��, �ش� �̹����� ������ �� ǥ���� 3D ������Ʈ�� ������ ������ ��ųʸ�
    private Dictionary<string, GameObject> _prefabDic = new Dictionary<string, GameObject>();

    // �̹��� �̸��� �ش� �г� ������Ʈ�� �����ϴ� ��ųʸ�
    private Dictionary<string, GameObject> imageToPanelMap = new Dictionary<string, GameObject>();

    public GameObject ObjectPool;
    public GameObject panelDangpa;    // "danpa" �г�
    public GameObject panelDeungpae; // "deungpae" �г�
    public GameObject panelGyojeon;  // "gyojeon" �г�
    public GameObject panelSsanggeom; // "ssanggeom" �г�
    public GameObject panelSsangsudo; // "ssangsudo" �г�
    public GameObject panelNangseon;  // "nangseon" �г�
    public GameObject panelWoldo;     // "woldo" �г�
    public GameObject panelHyeopdo;   // "hyeopdo" �г�
    public GameObject panelGwonbeop;  // "gwonbeop" �г�

    // ������Ʈ Ȯ�� �� ���
    private float previousTouchDistance = 0;
    private Vector3 initialScale;

    // ������Ʈ ȸ��
    private float previousTouchPositionX = 0f;
    private float startRotationY = 0f;

    void Awake()
    {
        // ������Ʈ ����Ʈ�� �ִ� 3D ������Ʈ�� �̸��� �Բ� ��ųʸ��� �߰�
        foreach (GameObject obj in _objectList)
        {
            string tName = obj.name;
            _prefabDic.Add(tName, obj); // ������Ʈ �̸��� Ű�� ��ųʸ��� ����
        }


        // �г� ������Ʈ�� ��ųʸ��� ����
        imageToPanelMap.Add("dangpa", panelDangpa);
        imageToPanelMap.Add("deungpae", panelDeungpae);
        imageToPanelMap.Add("gyojeon", panelGyojeon);
        imageToPanelMap.Add("ssanggeom", panelSsanggeom);
        imageToPanelMap.Add("ssangsudo", panelSsangsudo);
        imageToPanelMap.Add("nangseon", panelNangseon);
        imageToPanelMap.Add("woldo", panelWoldo);
        imageToPanelMap.Add("hyeopdo", panelHyeopdo);
        imageToPanelMap.Add("gwonbeop", panelGwonbeop);
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
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
            }
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage != null)
            {
                UpdateImage(trackedImage);

                if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited)
                {
                    DisableImage(trackedImage);
                }
            }
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            if (trackedImage != null)
            {
                DisableImage(trackedImage);
            }
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (_prefabDic.ContainsKey(name))
        {
            GameObject tObj = _prefabDic[name];
            tObj.transform.position = trackedImage.transform.position;
            tObj.SetActive(true);

        }

        if (imageToPanelMap.ContainsKey(name))
        {
            GameObject panel = imageToPanelMap[name];
            panel.SetActive(true); // �г� Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("'No prefab found for tracked image' or 'No panel found for tracked image': " + name);
        }
    }

    private void DisableImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (_prefabDic.ContainsKey(name))
        {
            GameObject tObj = _prefabDic[name];
            tObj.SetActive(false);
        }

        if (imageToPanelMap.ContainsKey(name))
        {
            GameObject panel = imageToPanelMap[name];
            panel.SetActive(false); // �г� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("'No prefab found for tracked image' or 'No panel found for tracked image': " + name);
        }
    }
}