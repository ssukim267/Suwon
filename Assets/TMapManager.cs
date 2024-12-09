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

            function initTmap() {
                map = new Tmapv2.Map('map_div', {
                    center : new Tmapv2.LatLng(37.56520450, 126.98702028),
                    width : '100%',
                    height : '800px',
                    zoom : 17,
                    zoomControl : true,
                    scrollwheel : true
                });

                marker_s = new Tmapv2.Marker({
                    position : new Tmapv2.LatLng(37.564991, 126.983937),
                    iconSize : new Tmapv2.Size(48, 76),
                    map : map
                });

                marker_e = new Tmapv2.Marker({
                    position : new Tmapv2.LatLng(37.566158, 126.988940),
                    iconSize : new Tmapv2.Size(48, 76),
                    map : map
                });

                var headers = {};
                headers['appKey'] = 'Wc3HLH9m4F2s8EVxGHGSI8bj0bfszFM4aAJii2L4';

                $.ajax({
                    method : 'POST',
                    headers : headers,
                    url : 'https://apis.openapi.sk.com/tmap/routes/pedestrian?version=1&format=json&callback=result',
                    async : false,
                    data : {
                        'startX' : '126.983937',
                        'startY' : '37.564991',
                        'endX' : '126.988940',
                        'endY' : '37.566158',
                        'reqCoordType' : 'WGS84GEO',
                        'resCoordType' : 'EPSG3857',
                        'startName' : 'Ãâ¹ßÁö',
                        'endName' : 'µµÂøÁö'
                    },
                    success : function(response) {
                        var resultData = response.features;
                        var tDistance = 'ÃÑ °Å¸® : ' + ((resultData[0].properties.totalDistance) / 1000).toFixed(1) + 'km,'; 
                        var tTime = ' ÃÑ ½Ã°£ : ' + ((resultData[0].properties.totalTime) / 60).toFixed(0) + 'ºÐ';
                        $('#result').text(tDistance + tTime);

                        // Clear previous markers if any
                        if (totalMarkerArr.length > 0) {
                            for (var i in totalMarkerArr) {
                                totalMarkerArr[i].setMap(null);
                            }
                            totalMarkerArr = [];
                        }

                        // Loop through the result data and create markers at each coordinate
                        for (var i in resultData) {
                            var geometry = resultData[i].geometry;
                            var properties = resultData[i].properties;

                            var latlng = new Tmapv2.Point(geometry.coordinates[0], geometry.coordinates[1]);
                            var convertPoint = new Tmapv2.Projection.convertEPSG3857ToWGS84GEO(latlng);

                            // Creating marker at each coordinate
                            var marker_p = new Tmapv2.Marker({
                                position : new Tmapv2.LatLng(convertPoint._lat, convertPoint._lng),
                                iconSize : new Tmapv2.Size(48, 76),
                                map : map
                            });
                            totalMarkerArr.push(marker_p); // Store marker for future use
                        }
                    },
                    error : function(request, status, error) {
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
