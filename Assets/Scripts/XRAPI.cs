using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Windows;
using TMPro;

using Random = UnityEngine.Random;

public class XRAPI : MonoBehaviour
{
    private const string URL = "http://xrproject.mushclam.com/predict";

    public Queue<string> audioQueue = new Queue<string>();

    public GameObject indicator;
    private List<GameObject> indicatorList = new List<GameObject>();

    private static readonly WaitForSeconds wait = new WaitForSeconds(1f);
    private static WaitWhile waitWhile;

    public IEnumerator InferenceAudioAndDraw(Canvas canvas)
    {
        RectTransform coord = canvas.transform as RectTransform;
        float range = coord.rect.height / 2;

        waitWhile = new WaitWhile(() => audioQueue.Count <= 0);

        while (true)
        {
            yield return waitWhile;
            var filepath = audioQueue.Dequeue();

            // Inference audio files
            string result = "";
            yield return StartCoroutine(
                ProcessRequest(filepath, (tag) =>
                {
                    result = tag;
                })
            );

            // Destroy previous objects
            foreach (GameObject obj in indicatorList)
            {
                Destroy(obj);
            }

            // Instantiate indicators
            GameObject text = Instantiate(indicator) as GameObject;
            text.transform.SetParent(canvas.transform, false);

            // Set text
            text.GetComponent<TextMeshProUGUI>().text = result;
            // Set Local Position
            float x = Random.Range(-1.0f, 1.0f) * range;
            float y = Random.Range(-1.0f, 1.0f) * range;
            text.transform.localPosition = new Vector2(x, y);
            indicatorList.Add(text);
        }
    }

    private IEnumerator ProcessRequest(string filepath, Action<string> callback)
    {
        // Audio file Information
        Byte[] bytes;
        if (File.Exists(filepath))
        {
            bytes = File.ReadAllBytes(filepath);
        }
        else
        {
            yield break;
        }
        string[] paths = filepath.Split("/");
        string filename = paths[paths.Length - 1];

        // Generate multipart data-form
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("audio_file", bytes, filename, "audio/wav"));

        // generate a boundary then convert the form to byte[]
        byte[] boundary = UnityWebRequest.GenerateBoundary();
        byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
        // my termination string consisting of CRLF--{boundary}--
        byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));
        // Make my complete body from the two byte arrays
        byte[] body = new byte[formSections.Length + terminate.Length];
        Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
        Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);
        // Set the content type -NO QUOTES around the boundary
        string contentType = String.Concat("multipart/form-data; boundary=\"", Encoding.UTF8.GetString(boundary), "\"");

        using (UnityWebRequest request = UnityWebRequest.Post(URL, formData, boundary))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.uploadHandler.contentType = contentType;
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", contentType);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                var audio_class = request.downloadHandler.text[2..^2];
                callback(audio_class);
            }
        }
    }
}
