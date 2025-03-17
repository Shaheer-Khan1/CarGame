using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

public class WebcamCapture : MonoBehaviour
{
    public RawImage display;
    private WebCamTexture webcamTexture;
    private string faceSavePath;
    private string screenshotSavePath;
    private string apiUrl = "http://localhost:3000/predict"; // Flask API endpoint

    public Text emotionResultText;

    void Start()
    {
        // Initialize file paths
        faceSavePath = Path.Combine(Application.persistentDataPath, "face.jpg");
        screenshotSavePath = Path.Combine(Application.persistentDataPath, "screenshot.jpg");

        // Check if a webcam is available
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("🚨 No camera found!");
            return;
        }

        Debug.Log("Camera found!");
        webcamTexture = new WebCamTexture(); // Use default resolution and frame rate

        if (webcamTexture != null)
        {
            display.texture = webcamTexture;
            webcamTexture.Play();

            if (webcamTexture.isPlaying)
            {
                Debug.Log("✅ Webcam started.");
                InvokeRepeating("CaptureAndProcessFrame", 0f, 30f); // Capture every 30 seconds
            }
            else
            {
                Debug.LogError("❌ Failed to start webcam. Is the webcam in use or not accessible?");
            }
        }
        else
        {
            Debug.LogError("❌ WebcamTexture is null. Check if the webcam is properly connected.");
        }
    }

    void CaptureAndProcessFrame()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying)
        {
            Debug.LogError("❌ Cannot capture frame - Webcam not initialized!");
            return;
        }
        Debug.Log("📸 Capturing frame and screenshot...");
        StartCoroutine(SaveAndSendImage());
    }

    IEnumerator SaveAndSendImage()
    {
        yield return new WaitForEndOfFrame();

        // Capture webcam image
        Texture2D webcamSnap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
        webcamSnap.SetPixels(webcamTexture.GetPixels());
        webcamSnap.Apply();
        byte[] webcamImageData = webcamSnap.EncodeToJPG(50); // Reduced quality
        File.WriteAllBytes(faceSavePath, webcamImageData);
        Debug.Log("✅ Face image saved to " + faceSavePath);

        // Capture screenshot
        Texture2D screenshotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshotTexture.Apply();
        byte[] screenshotData = screenshotTexture.EncodeToJPG(50); // Reduced quality
        File.WriteAllBytes(screenshotSavePath, screenshotData);
        Debug.Log("✅ Screenshot saved to " + screenshotSavePath);

        // Send images to API
        StartCoroutine(SendImagesToAPI(webcamImageData, screenshotData));

        // Clean up
        Destroy(webcamSnap);
        Destroy(screenshotTexture);
    }

    IEnumerator SendImagesToAPI(byte[] faceImageData, byte[] screenshotData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", faceImageData, "face.jpg", "image/jpeg");
        form.AddBinaryData("screenshot", screenshotData, "screenshot.jpg", "image/jpeg");

        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
        {
            Debug.Log("📡 Sending images to API...");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Error sending images: {request.error}");
                if (emotionResultText != null)
                {
                    emotionResultText.text = $"Error: {request.error}";
                }
            }
            else
            {
                string response = request.downloadHandler.text;
                Debug.Log($"✅ API Response: {response}");
                DisplayEmotionResults(response);
            }
        }
    }

    void DisplayEmotionResults(string jsonResponse)
    {
        try
        {
            if (jsonResponse.Contains("\"dominant\":"))
            {
                int emotionStart = jsonResponse.IndexOf("\"dominant\":") + "\"dominant\":".Length;
                int emotionEnd = jsonResponse.IndexOf("\"", emotionStart + 1);

                if (emotionEnd > emotionStart)
                {
                    string dominantEmotion = jsonResponse.Substring(emotionStart + 1, emotionEnd - emotionStart - 1);

                    if (emotionResultText != null)
                    {
                        emotionResultText.text = $"Emotion: {dominantEmotion}";
                    }

                    Debug.Log($"😀 Dominant emotion: {dominantEmotion}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing emotion results: {e.Message}");
        }
    }

    void OnDestroy()
    {
        // Clean up webcam texture when the object is destroyed
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}