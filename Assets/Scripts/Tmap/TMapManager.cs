/* ����� ����ġ-->������ */
using System.Collections;
using UnityEngine;

public class TMapManager : MonoBehaviour
{
    private WebViewObject webViewObject;
    private float startLatitude; // �⺻ ����� ����
    private float startLongitude; // �⺻ ����� �浵

    // GPSLocation �ν��Ͻ� ����
    public GPSlocation gpsLocation;

    private void Start()
    {
        if (gpsLocation != null)
        {
            // ��ġ ������Ʈ�� �Ϸ�� �� ó���ϵ��� �̺�Ʈ �ڵ鷯 �߰�
            gpsLocation.OnLocationUpdated += OnLocationUpdated;
        }
        else
        {
            Debug.LogError("GPSLocation ��ü�� null�Դϴ�. GPSLocation�� �Ҵ��� �ּ���.");
        }

    }

    // ��ġ ������ ������Ʈ�Ǹ� ȣ��Ǵ� �޼���
    private void OnLocationUpdated()
    {
        startLatitude = gpsLocation.latitude;
        startLongitude = gpsLocation.longitude;
        Debug.Log("gps ��ũ��Ʈ���� �޾ƿ� ����� ��ǥ: " + startLatitude + " " + startLongitude);

        StartWebView();
    }


    public void StartWebView()
    {
        // ������ �浵�� ��ȿ���� Ȯ��
        if (startLatitude == 0 || startLongitude == 0)
        {
            Debug.LogWarning("GPS ��ġ ������ ���� ������Ʈ���� �ʾҽ��ϴ�.");
            return; // GPS ������ ������ WebView�� �������� ����
        }

        // HTML Map content as a string
        string mapHtml = @"
        <!DOCTYPE html>
        <html>
        <head>
        <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
        <title>simpleMap</title>
        <script src='https://code.jquery.com/jquery-3.2.1.min.js'></script>
        <script src='https://apis.openapi.sk.com/tmap/jsv2?version=1&appKey=Wc3HLH9m4F2s8EVxGHGSI8bj0bfszFM4aAJii2L4'></script>
        <script type='text/javascript'>
    var map;
    var marker_s, marker_e;
    var totalMarkerArr = [];
    var polyline; // Polyline ��ü�� ����

    function initTmap() {
        map = new Tmapv2.Map('map_div', {
            position: new Tmapv2.LatLng(" + startLatitude + @", " + startLongitude + @"),
            width: '100%',
            height: '100vh',
            zoom: 17,
            zoomControl: true,
            scrollwheel: true
        });

        marker_s = new Tmapv2.Marker({
            position: new Tmapv2.LatLng(" + startLatitude + @", " + startLongitude + @"),
            icon: 'http://topopen.tmap.co.kr/imgs/start.png',
            iconSize: new Tmapv2.Size(90, 90),
            map: map
        });

        marker_e = new Tmapv2.Marker({
            position: new Tmapv2.LatLng(37.279598, 127.042719),
            icon: 'http://topopen.tmap.co.kr/imgs/arrival.png',
            iconSize: new Tmapv2.Size(90, 90),
            map: map
        });

        var headers = {};
        headers['appKey'] = 'Wc3HLH9m4F2s8EVxGHGSI8bj0bfszFM4aAJii2L4';

        $.ajax({
            method: 'POST',
            headers: headers,
            url: 'https://apis.openapi.sk.com/tmap/routes/pedestrian?version=1&format=json&callback=result',
            async: false,
            data: {
                'startX': '" + startLongitude + @"',
                'startY': '" + startLatitude + @"',
                'endX': '127.042719',
                'endY': '37.279598',
                'reqCoordType': 'WGS84GEO',
                'resCoordType': 'EPSG3857',
                'startName': '�����',
                'endName': '������',
            },
            success: function(response) {
                var resultData = response.features;


                // Polyline�� �׸��� ���� ��ǥ �迭
                var path = [];

                for (var i in resultData) {
                    var geometry = resultData[i].geometry;

                    if (geometry.type == 'LineString') {
                        for (var j in geometry.coordinates) {
                            var latlng = new Tmapv2.Point(geometry.coordinates[j][0], geometry.coordinates[j][1]);
                            var convertPoint = new Tmapv2.Projection.convertEPSG3857ToWGS84GEO(latlng);
                            path.push(new Tmapv2.LatLng(convertPoint._lat, convertPoint._lng));
                        }
                    }
                }

                // ���� Polyline ����
                if (polyline) {
                    polyline.setMap(null);
                }

                // Polyline ���� �� �߰�
                polyline = new Tmapv2.Polyline({
                    path: path, // ��� ����
                    strokeColor: '#FF0000', // �� ����
                    strokeWeight: 15, // �� �β�
                    map: map // ������ �߰�
                });
            },
            error: function(request, status, error) {
                console.log('code:' + request.status + '\n' + 'message:' + request.responseText + '\n' + 'error:' + error);
            }
        });
    }
</script>

        </head>
        <body onload='initTmap();'>
            <div id='map_wrap' class='map_wrap3'>
                <div id='map_div'></div>
            </div>
            <div id='result'></div>
        </body>
        </html>";

        try
        {
            if (webViewObject == null)
            {
                webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
                webViewObject.Init((msg) =>
                {
                    Debug.Log(string.Format("CallFromJS[{0}]", msg));
                });

                // Inject HTML content into WebView
                webViewObject.LoadURL("data:text/html;base64," + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(mapHtml)));
                webViewObject.SetVisibility(true);

                // Set WebView margins
                webViewObject.SetMargins(0, 200, 0, 0);
            }
            else
            {
                webViewObject.SetVisibility(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WebView Error : {e}");
        }
    }
}

/* �����(�ϵ��ڵ�) --> ������(�ϵ��ڵ�) */

/*
using System.Collections;
using UnityEngine;

public class TMapManager : MonoBehaviour
{
    private WebViewObject webViewObject;

    private void Start()
    {
        StartWebView();
    }

    public void StartWebView()
    {
        // HTML Map content as a string
        string mapHtml = @"
        <!DOCTYPE html>
        <html>
        <head>
        <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
        <title>simpleMap</title>
        <script src='https://code.jquery.com/jquery-3.2.1.min.js'></script>
        <script src='https://apis.openapi.sk.com/tmap/jsv2?version=1&appKey=Wc3HLH9m4F2s8EVxGHGSI8bj0bfszFM4aAJii2L4'></script>
        <script type='text/javascript'>
    var map;
    var marker_s, marker_e;
    var totalMarkerArr = [];
    var polyline; // Polyline ��ü�� ����

    function initTmap() {
        map = new Tmapv2.Map('map_div', {
            center: new Tmapv2.LatLng(37.279111,127.042891),
            width: '100%',
            height: '100vh',
            zoom: 17,
            zoomControl: true,
            scrollwheel: true
        });

        marker_s = new Tmapv2.Marker({
            position: new Tmapv2.LatLng(37.279111, 127.042891),
            iconSize: new Tmapv2.Size(48, 76),
            map: map
        });

        marker_e = new Tmapv2.Marker({
            position: new Tmapv2.LatLng(37.277466, 127.044024),
            iconSize: new Tmapv2.Size(48, 76),
            map: map
        });

        var headers = {};
        headers['appKey'] = 'Wc3HLH9m4F2s8EVxGHGSI8bj0bfszFM4aAJii2L4';

        $.ajax({
            method: 'POST',
            headers: headers,
            url: 'https://apis.openapi.sk.com/tmap/routes/pedestrian?version=1&format=json&callback=result',
            async: false,
            data: {
                'startX': '127.042891',
                'startY': '37.279111',
                'endX': '127.044024',
                'endY': '37.277466',
                'reqCoordType': 'WGS84GEO',
                'resCoordType': 'EPSG3857',
                'startName': '�����',
                'endName': '������',
            },
            success: function(response) {
                var resultData = response.features;

                // �Ÿ��� �ð� ���
                var tDistance = '�� �Ÿ� : ' + ((resultData[0].properties.totalDistance) / 1000).toFixed(1) + 'km,';
                var tTime = ' �� �ð� : ' + ((resultData[0].properties.totalTime) / 60).toFixed(0) + '��';
                $('#result').text(tDistance + tTime);

                // Polyline�� �׸��� ���� ��ǥ �迭
                var path = [];

                for (var i in resultData) {
                    var geometry = resultData[i].geometry;

                    if (geometry.type == 'LineString') {
                        for (var j in geometry.coordinates) {
                            var latlng = new Tmapv2.Point(geometry.coordinates[j][0], geometry.coordinates[j][1]);
                            var convertPoint = new Tmapv2.Projection.convertEPSG3857ToWGS84GEO(latlng);
                            path.push(new Tmapv2.LatLng(convertPoint._lat, convertPoint._lng));
                        }
                    }
                }

                // ���� Polyline ����
                if (polyline) {
                    polyline.setMap(null);
                }

                // Polyline ���� �� �߰�
                polyline = new Tmapv2.Polyline({
                    path: path, // ��� ����
                    strokeColor: '#FF0000', // �� ����
                    strokeWeight: 15, // �� �β�
                    map: map // ������ �߰�
                });
            },
            error: function(request, status, error) {
                console.log('code:' + request.status + '\n' + 'message:' + request.responseText + '\n' + 'error:' + error);
            }
        });
    }
</script>

        </head>
        <body onload='initTmap();'>
            <div id='map_wrap' class='map_wrap3'>
                <div id='map_div'></div>
            </div>
            <div id='result'></div>
        </body>
        </html>";

        try
        {
            if (webViewObject == null)
            {
                webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
                webViewObject.Init((msg) =>
                {
                    Debug.Log(string.Format("CallFromJS[{0}]", msg));
                });

                // Inject HTML content into WebView
                webViewObject.LoadURL("data:text/html;base64," + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(mapHtml)));
                webViewObject.SetVisibility(true);

                // Set WebView margins
                webViewObject.SetMargins(100, 100, 100, 100);
            }
            else
            {
                webViewObject.SetVisibility(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WebView Error : {e}");
        }
    }
}
*/