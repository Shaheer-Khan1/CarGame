using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CarMaterialChanger : MonoBehaviour
{
    // Fill these in the Inspector
    public Renderer carRenderer;
    public Material blueMaterial;
    public Material greenMaterial;
    public Material whiteMaterial;
    public Material greyMaterial;
    public Material purpleMaterial;
    
    void Start()
    {
        // Hard-coded response for testing - remove this line when using the API
        ApplyColor("green");
        
        // Uncomment this line to use the API
        //StartCoroutine(GetColorFromAPI());
    }
    
    IEnumerator GetColorFromAPI()
    {
        using (UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:5000/predict"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log("API Response: " + response);
                
                // Extract color from response text
                if (response.Contains("blue"))
                    ApplyColor("blue");
                else if (response.Contains("green"))
                    ApplyColor("green");
                else if (response.Contains("white"))
                    ApplyColor("white");
                else if (response.Contains("grey") || response.Contains("gray"))
                    ApplyColor("grey");
                else if (response.Contains("purple"))
                    ApplyColor("purple");
                else
                    Debug.Log("Unknown color in response: " + response);
            }
            else
            {
                Debug.LogError("API Error: " + request.error);
            }
        }
    }
    
    void ApplyColor(string color)
    {
        Debug.Log("Applying color: " + color);
        
        switch (color)
        {
            case "blue":
                carRenderer.material = blueMaterial;
                break;
            case "green":
                carRenderer.material = greenMaterial;
                break;
            case "white":
                carRenderer.material = whiteMaterial;
                break;
            case "grey":
                carRenderer.material = greyMaterial;
                break;
            case "purple":
                carRenderer.material = purpleMaterial;
                break;
        }
    }
}