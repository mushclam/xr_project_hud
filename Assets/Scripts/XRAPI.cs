using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows;
using TMPro;

public class XRAPI : MonoBehaviour
{
    private const string URL = "http://xrproject.mushclam.com/predict";

    public string audio_name;
    public string audio_class;
    public TextMeshProUGUI tmp;
    // Start is called before the first frame update
    private void Update()
    {
        tmp.text = audio_class;
    }

    public void GenerateRequest()
    {
        StartCoroutine(ProcessRequest(URL));
    }

    private IEnumerator ProcessRequest(string uri)
    {
        // Audio file Information
        string filepath = "Assets/Audio/" + audio_name;
        var bytes = File.ReadAllBytes(filepath);
        var filename = audio_name;

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
        Debug.Log(contentType);

        UnityWebRequest request = UnityWebRequest.Post(uri, formData, boundary);
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
            audio_class = request.downloadHandler.text[2..^2];
            Debug.Log(audio_class);
        }
    }
}
