using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Gley.TrafficSystem.Internal
{
    public class CameraFollow : MonoBehaviour
    {
        public bool shouldRotate = true;

        // Assign these in the Unity Inspector
        public Transform blueCar;
        public Transform greyCar;
        public Transform whiteCar;
        public Transform purpleCar;
        public Transform greenCar;

        private Transform currentTarget;

        // Distance and height settings
        public float distance = 10.0f;
        public float height = 5.0f;
        public float heightDamping = 2.0f;
        public float rotationDamping = 3.0f;

        private string apiUrl = "http://127.0.0.1:5000/get_prediction/test_user"; // Update if needed

        void Start()
        {
            StartCoroutine(GetPrediction());
        }

        IEnumerator GetPrediction()
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

        void SelectCar(string color)
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

            FollowSelectedCar(selectedCar);
        }

        void FollowSelectedCar(Transform targetCar)
        {
            if (targetCar == null)
            {
                Debug.LogError("Selected car does not exist!");
                return;
            }

            currentTarget = targetCar;

            // Hide all other cars instead of destroying them
            List<Transform> allCars = new List<Transform> { blueCar, greyCar, whiteCar, purpleCar, greenCar };
            foreach (Transform car in allCars)
            {
                if (car != null)
                {
                    car.gameObject.SetActive(car == currentTarget); // Only keep the selected car visible
                }
            }

            Debug.Log("Following car: " + currentTarget.name);
        }

        void FixedUpdate()
        {
            if (currentTarget == null)
                return;

            float wantedRotationAngle = currentTarget.eulerAngles.y;
            float wantedHeight = currentTarget.position.y + height;
            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;

            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            transform.position = currentTarget.position - (currentRotation * Vector3.forward * distance);
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            if (shouldRotate)
                transform.LookAt(currentTarget);
        }

        [System.Serializable]
        private class PredictionResponse
        {
            public string username;
            public string recommended_car_color;
        }
    }
}
