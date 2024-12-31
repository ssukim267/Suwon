using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class ARwithAudio : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    // 3D 오브젝트 리스트
    public List<GameObject> _objectList = new List<GameObject>();

    // 이미지 이름을 키로, 해당 이미지를 추적할 때 표시할 3D 오브젝트를 값으로 가지는 딕셔너리
    private Dictionary<string, GameObject> _prefabDic = new Dictionary<string, GameObject>();

    // 이미지 이름과 해당 패널 오브젝트를 매핑하는 딕셔너리
    private Dictionary<string, GameObject> imageToPanelMap = new Dictionary<string, GameObject>();

    public GameObject ObjectPool;
    public GameObject panelDangpa;    // "danpa" 패널
    public GameObject panelDeungpae; // "deungpae" 패널
    public GameObject panelGyojeon;  // "gyojeon" 패널
    public GameObject panelSsanggeom; // "ssanggeom" 패널
    public GameObject panelSsangsudo; // "ssangsudo" 패널
    public GameObject panelNangseon;  // "nangseon" 패널
    public GameObject panelWoldo;     // "woldo" 패널
    public GameObject panelHyeopdo;   // "hyeopdo" 패널
    public GameObject panelGwonbeop;  // "gwonbeop" 패널

    // 오브젝트 확대 및 축소
    private float previousTouchDistance = 0;
    private Vector3 initialScale;

    // 오브젝트 회전
    private float previousTouchPositionX = 0f;
    private float startRotationY = 0f;

    void Awake()
    {
        // 오브젝트 리스트에 있는 3D 오브젝트를 이름과 함께 딕셔너리에 추가
        foreach (GameObject obj in _objectList)
        {
            string tName = obj.name;
            _prefabDic.Add(tName, obj); // 오브젝트 이름을 키로 딕셔너리에 저장
        }


        // 패널 오브젝트를 딕셔너리에 매핑
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
            panel.SetActive(true); // 패널 활성화
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
            panel.SetActive(false); // 패널 비활성화
        }
        else
        {
            Debug.LogWarning("'No prefab found for tracked image' or 'No panel found for tracked image': " + name);
        }
    }
}