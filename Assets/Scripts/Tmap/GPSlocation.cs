using UnityEngine;
using System.Collections;

public class GPSlocation : MonoBehaviour
{
    public float latitude { get; private set; }
    public float longitude { get; private set; }

    // ��ġ ������ �غ�Ǿ��� �� ȣ��� �̺�Ʈ
    public event System.Action OnLocationUpdated;

    IEnumerator Start()
    {
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
            Debug.Log("Location not enabled on device or app does not have permission to access location");

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            Debug.Log("Location: " + latitude + " " + longitude);

            // ��ġ ������ ������Ʈ �Ǿ��� �� �̺�Ʈ ȣ��
            OnLocationUpdated?.Invoke();
        }

        // Stops the location service if there is no need to query location updates continuously.
        Input.location.Stop();
    }
}