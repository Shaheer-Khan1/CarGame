using UnityEngine;
using TMPro;
using System.Collections; // Required for using Coroutines
using System.Collections.Generic; // Required for using List<>
using UnityEngine.Networking;

public class TrafficLightPrompt : MonoBehaviour
{
    public GameObject redLightOn;           // Reference to the "RedLightOn" GameObject (the red light)
    public TextMeshProUGUI redLightPrompt;  // Reference to the TextMeshPro component in the Canvas
    public GameObject canvas;               // Reference to the Canvas GameObject for the prompt
    public float detectionRange = 10f;      // The distance range within which the prompt will show
    public GameObject boxObject;            // Reference to the box (cube) object, representing the line of the red light
    public float collisionThreshold = 0.5f; // Threshold distance to consider as a "collision" with the cube

    // Car references (assign these in the Unity Inspector)
    public Transform blueCar;
    public Transform greyCar;
    public Transform whiteCar;
    public Transform purpleCar;
    public Transform greenCar;

    private Transform playerCar;            // Reference to the player's car (or player object)
    private bool promptVisible = false;     // To track the visibility state of the prompt
    private Coroutine hidePromptCoroutine;  // To store the coroutine that hides the prompt

    private string apiUrl = "http://127.0.0.1:5000/get_prediction/test_user"; // Update if needed

    private void Start()
    {
        // Ensure the prompt and the canvas are hidden initially
        canvas.SetActive(false);

        // Set text properties (color and size)
        redLightPrompt.color = Color.white;    // Set the text color to white
        redLightPrompt.fontSize = 36;          // Set the text size to 36 (adjust as needed)

        // Start the API call to get the recommended car
        StartCoroutine(GetPrediction());

        Debug.Log("Prompt initialized, hidden, color set to white, and font size set to 36.");
    }

    private IEnumerator GetPrediction()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching prediction: " + request.error);
                yield break; // Stop execution on failure
            }

            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);

            // Parse JSON safely
            PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(jsonResponse);

            if (response == null || string.IsNullOrEmpty(response.recommended_car_color))
            {
                Debug.LogError("Invalid or empty JSON response!");
                yield break; // Stop execution if response is invalid
            }

            Debug.Log("Recommended Car Color: " + response.recommended_car_color);
            SelectCar(response.recommended_car_color);
        }
    }

    private void SelectCar(string color)
    {
        Transform selectedCar = null;

        switch (color.ToLower())
        {
            case "blue":
                selectedCar = blueCar;
                break;
            case "grey":
                selectedCar = greyCar;
                break;
            case "white":
                selectedCar = whiteCar;
                break;
            case "purple":
                selectedCar = purpleCar;
                break;
            case "green":
                selectedCar = greenCar;
                break;
            default:
                Debug.LogError("No matching car color found!");
                return;
        }

        // Set the selected car as the player car
        playerCar = selectedCar;

        // Hide all other cars
        List<Transform> allCars = new List<Transform> { blueCar, greyCar, whiteCar, purpleCar, greenCar };
        foreach (Transform car in allCars)
        {
            if (car != null)
            {
                car.gameObject.SetActive(car == playerCar); // Only keep the selected car visible
            }
        }

        Debug.Log("Selected car: " + playerCar.name);
    }

    private void Update()
    {
        // If the player car is not assigned yet, skip the update logic
        if (playerCar == null)
        {
            Debug.LogWarning("Player car not assigned yet.");
            return;
        }

        // Calculate the distance between the player car and the red light
        float distanceToRedLight = Vector3.Distance(playerCar.position, redLightOn.transform.position);

        // Show the "Please wait, light is red" prompt if within range and the red light is ON
        if (redLightOn.activeInHierarchy && distanceToRedLight <= detectionRange)
        {
            if (!canvas.activeSelf)
            {
                canvas.SetActive(true);               // Show the entire canvas
                redLightPrompt.text = "Please wait, light is red";  // Set the text
                Debug.Log("Red light is ON. Player within range. Prompt activated: 'Please wait, light is red'.");
            }
        }
        else
        {
            // Hide the canvas and the prompt when the red light is off or the player is out of range
            if (canvas.activeSelf)
            {
                canvas.SetActive(false);              // Hide the entire canvas
                redLightPrompt.text = "";             // Clear the text
                Debug.Log("Red light is OFF or player out of range. Prompt hidden, text cleared.");
            }
        }

        // Check the distance between the player car and the box (representing the red light crossing line)
        float distanceToBox = Vector3.Distance(playerCar.position, boxObject.transform.position);

        // Collision logic only works when the red light is ON
        if (redLightOn.activeInHierarchy && distanceToBox <= collisionThreshold && !promptVisible)
        {
            Debug.Log("Car has crossed the red light based on distance!");

            // Change the prompt to indicate that the red light was broken
            redLightPrompt.text = "You broke the red light!";
            promptVisible = true;

            // Start the coroutine to hide the prompt after 3 seconds
            if (hidePromptCoroutine != null)
            {
                StopCoroutine(hidePromptCoroutine);  // In case it's already running, stop it first
            }
            hidePromptCoroutine = StartCoroutine(HidePromptAfterDelay(3f)); // Hide after 3 seconds
        }
    }

    // Coroutine to hide the prompt after a specified delay
    private IEnumerator HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // Wait for the specified delay

        // Hide the canvas and clear the text
        canvas.SetActive(false);
        redLightPrompt.text = "";
        promptVisible = false;

        Debug.Log("Prompt hidden after 3 seconds.");
    }

    [System.Serializable]
    private class PredictionResponse
    {
        public string username;
        public string recommended_car_color;
    }
}