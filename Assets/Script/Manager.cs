using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ButtonInteraction : MonoBehaviour
{
    public GameObject genMesh; // Reference to the genMesh GameObject
    public Material genMeshMaterial; // Reference to the material
    public Button buttonPrefab; // Reference to the button prefab
    public Canvas canvas; // Reference to the canvas
    private int buttonClickCount = 0; // Counter to track button clicks
    private string[] emotions = new string[]
    {
        "Ecstasy", "Elation", "Triumph", "+surprise", "-surprise", "Amusement",
        "Contentment", "Sympathy", "Interest", "Realize", "Adoration", "Relief",
        "Desire", "Confusion", "Embarrassment", "Fear", "Distress", "Disgust",
        "Disappointment", "Contempt", "Pain", "Awe", "Anger", "Sadness"
    };

    private Button[] instantiatedButtons; // To store instantiated buttons
    private Vector2[] velocities; // To store the movement velocities for each button
    public float buttonSpeed = 100f; // Speed of button movement
    public float maxDistanceFromCenter = 300f; // Maximum distance a button can move from the center

    private RectTransform canvasRect; // The canvas boundary for detecting collisions

    private void Start()
    {
        // Get the canvas RectTransform and use its pivot point as the center
        canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 canvasCenter = canvasRect.rect.center; // This gives the center in local space

        instantiatedButtons = new Button[emotions.Length];
        velocities = new Vector2[emotions.Length];

        // Instantiate buttons at the center of the canvas
        for (int i = 0; i < emotions.Length; i++)
        {
            Button button = Instantiate(buttonPrefab, canvas.transform);

            // Reset button scale to 1 for proper size handling
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            //rectTransform.localScale = Vector3.one;

            // Set button text
            button.GetComponentInChildren<TextMeshProUGUI>().text = emotions[i];
            button.onClick.AddListener(() => OnEmotionButtonClick());

            // Store the instantiated button
            instantiatedButtons[i] = button;

            // Set random initial velocity
            velocities[i] = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * buttonSpeed;

            // Calculate a random position from the center up to maxDistanceFromCenter
            Vector2 randomOffset = Random.insideUnitCircle * maxDistanceFromCenter;
            rectTransform.anchoredPosition = canvasCenter + randomOffset;

            // Set anchors to the center for proper positioning
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        }

        // Hide genMesh object initially
        genMesh.SetActive(false);
    }

    // Method called when an emotion button is clicked
    private void OnEmotionButtonClick()
    {
        buttonClickCount++;

        if (buttonClickCount >= 4)
        {
            StartCoroutine(ResetCounterAndActivateGenMesh());
        }
    }

    // Coroutine to reset the button click counter and activate genMesh
    private IEnumerator ResetCounterAndActivateGenMesh()
    {
        // Reset the button click counter
        buttonClickCount = 0;

        // Set a random seed value for the MeshGeneration script
        MeshGeneratorV1 meshGen = genMesh.GetComponent<MeshGeneratorV1>();
        meshGen.seed = Random.Range(100, 200);
        meshGen.octaves = Random.Range(2, 10);
        meshGen.lacunarity = Random.Range(2, 10);

        // Assign a random color to the material
        genMeshMaterial.color = new Color(Random.value, Random.value, Random.value);

        // Enable genMesh object and its MeshGeneration script
        genMesh.SetActive(true);

        // Hide the canvas when genMesh is activated
        canvas.gameObject.SetActive(false);

        // Wait for a second before completing the coroutine
        yield return new WaitForSeconds(1f);
    }

    private void Update()
    {
        // Animate each button by moving them randomly and constraining their movement around the canvas center
        for (int i = 0; i < instantiatedButtons.Length; i++)
        {
            Button button = instantiatedButtons[i];
            RectTransform rectTransform = button.GetComponent<RectTransform>();

            // Move the button by its velocity
            rectTransform.anchoredPosition += velocities[i] * Time.deltaTime;

            // Ensure the button doesn't move too far from the center
            Vector2 offset = rectTransform.anchoredPosition - canvasRect.rect.center; // Calculate distance from center
            if (offset.magnitude > maxDistanceFromCenter)
            {
                // Clamp the position to the maximum distance from the center
                offset = offset.normalized * maxDistanceFromCenter;
                rectTransform.anchoredPosition = canvasRect.rect.center + offset;

                // Reverse the velocity to simulate a bounce effect
                velocities[i] = -velocities[i];
            }
        }

        // Listen for the space key to reset the UI
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Deactivate genMesh and bring back the UI
            genMesh.SetActive(false);
            canvas.gameObject.SetActive(true); // Re-enable the canvas/UI
        }
    }
}
