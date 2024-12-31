/* 사용자 현위치-->도착지 */
using System.Collections;
using UnityEngine;

public class TMapManager : MonoBehaviour
{
    private WebViewObject webViewObject;
    private float startLatitude; // 기본 출발지 위도
    private float startLongitude; // 기본 출발지 경도

    // GPSLocation 인스턴스 참조
    public GPSlocation gpsLocation;

    private void Start()
    {
        if (gpsLocation != null)
        {
            // 위치 업데이트가 완료된 후 처리하도록 이벤트 핸들러 추가
            gpsLocation.OnLocationUpdated += OnLocationUpdated;
        }
        else
        {
            Debug.LogError("GPSLocation 객체가 null입니다. GPSLocation을 할당해 주세요.");
        }

    }

    // 위치 정보가 업데이트되면 호출되는 메서드
    private void OnLocationUpdated()
    {
        startLatitude = gpsLocation.latitude;
        startLongitude = gpsLocation.longitude;
        Debug.Log("gps 스크립트에서 받아온 사용자 좌표: " + startLatitude + " " + startLongitude);

        StartWebView();
    }


    public void StartWebView()
    {
        // 위도와 경도가 유효한지 확인
        if (startLatitude == 0 || startLongitude == 0)
        {
            Debug.LogWarning("GPS 위치 정보가 아직 업데이트되지 않았습니다.");
            return; // GPS 정보가 없으면 WebView를 시작하지 않음
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
    var polyline; // Polyline 객체를 저장

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
                'startName': '출발지',
                'endName': '도착지',
            },
            success: function(response) {
                var resultData = response.features;


                // Polyline을 그리기 위한 좌표 배열
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

                // 기존 Polyline 삭제
                if (polyline) {
                    polyline.setMap(null);
                }

                // Polyline 생성 및 추가
                polyline = new Tmapv2.Polyline({
                    path: path, // 경로 설정
                    strokeColor: '#FF0000', // 선 색깔
                    strokeWeight: 15, // 선 두께
                    map: map // 지도에 추가
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

/* 출발지(하드코딩) --> 도착지(하드코딩) */

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
    var polyline; // Polyline 객체를 저장

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
                'startName': '출발지',
                'endName': '도착지',
            },
            success: function(response) {
                var resultData = response.features;

                // 거리와 시간 출력
                var tDistance = '총 거리 : ' + ((resultData[0].properties.totalDistance) / 1000).toFixed(1) + 'km,';
                var tTime = ' 총 시간 : ' + ((resultData[0].properties.totalTime) / 60).toFixed(0) + '분';
                $('#result').text(tDistance + tTime);

                // Polyline을 그리기 위한 좌표 배열
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

                // 기존 Polyline 삭제
                if (polyline) {
                    polyline.setMap(null);
                }

                // Polyline 생성 및 추가
                polyline = new Tmapv2.Polyline({
                    path: path, // 경로 설정
                    strokeColor: '#FF0000', // 선 색깔
                    strokeWeight: 15, // 선 두께
                    map: map // 지도에 추가
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