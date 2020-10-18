using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class StatsTransmitter : MonoBehaviour
{
    public string statsUrl = "https://sennio.bplaced.net/ld47/stats.php";
    
    private Guid sessionId;

    // Start is called before the first frame update
    void Start()
    {
        sessionId = Guid.NewGuid();

        StartCoroutine(TransmitStats());
    }

    IEnumerator TransmitStats()
    {
        WWWForm form = new WWWForm();
        form.AddField("sessionId", sessionId.ToString());
        form.AddField("platform", Application.platform.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(statsUrl, form))
        {
            yield return www.SendWebRequest();

            // if (www.isNetworkError || www.isHttpError)
            // {
            //     Debug.Log(www.error);
            // }
            // else
            // {
            //     Debug.Log("Stats upload complete!");
            // }
        }
    }
}
