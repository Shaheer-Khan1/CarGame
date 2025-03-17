using UnityEngine;
using TMPro;
using System.Collections; // Required for using Coroutines

public class TrafficLightPrompt : MonoBehaviour
{
    public GameObject redLightOn;           // Reference to the "RedLightOn" GameObject (the red light)
    public TextMeshProUGUI redLightPrompt;  // Reference to the TextMeshPro component in the Canvas
    public GameObject canvas;               // Reference to the Canvas GameObject for the prompt
    public Transform playerCar;             // Reference to the player's car (or player object)
    public float detectionRange = 10f;      // The distance range within which the prompt will show
    public GameObject boxObject;            // Reference to the box (cube) object, representing the line of the red light
    public float collisionThreshold = 0.5f; // Threshold distance to consider as a "collision" with the cube

    private bool promptVisible = false;     // To track the visibility state of the prompt
    private Coroutine hidePromptCoroutine;  // To store the coroutine that hides the prompt

    private void Start()
    {
        // Ensure the prompt and the canvas are hidden initially
        canvas.SetActive(false);

        // Set text properties (color and size)
        redLightPrompt.color = Color.white;    // Set the text color to white
        redLightPrompt.fontSize = 36;          // Set the text size to 36 (adjust as needed)

        Debug.Log("Prompt initialized, hidden, color set to white, and font size set to 36.");
    }

    private void Update()
    {
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
}
