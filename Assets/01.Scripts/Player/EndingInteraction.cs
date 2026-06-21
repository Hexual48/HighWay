using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public sealed class EndingInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private GameObject interactButton;
    [SerializeField] private string endingSceneName = "Ending";

    private bool playerInside;
    private bool isLoading;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (interactButton == null)
        {
            Transform buttonTransform = transform.parent != null
                ? transform.parent.Find("Interect")
                : transform.Find("Interect");
            interactButton = buttonTransform != null ? buttonTransform.gameObject : null;
        }

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside || isLoading || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            isLoading = true;
            SceneManager.LoadScene(endingSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInside = true;

        if (interactButton != null)
        {
            interactButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInside = false;

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.transform.root.CompareTag("Player");
    }
}
