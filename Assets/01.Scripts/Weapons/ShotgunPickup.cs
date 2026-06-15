using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ShotgunPickup : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject interactButton;
    [SerializeField] private GameObject displayShotgun;

    private GameObject playerHandle;
    private bool playerInside;
    private bool pickedUp;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;

        if (interactButton == null)
        {
            Transform buttonTransform = transform.Find("Interect");
            interactButton = buttonTransform != null ? buttonTransform.gameObject : null;
        }

        if (displayShotgun == null)
        {
            Transform shotgunTransform = transform.Find("Shotgun");
            displayShotgun = shotgunTransform != null ? shotgunTransform.gameObject : null;
        }

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside || pickedUp || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            PickUpShotgun();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
        {
            return;
        }

        Transform player = GetPlayerTransform(other);

        if (player == null)
        {
            return;
        }

        playerHandle = FindChildByName(player, "Handle");
        playerInside = true;

        if (interactButton != null)
        {
            interactButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform player = GetPlayerTransform(other);

        if (player == null)
        {
            return;
        }

        playerInside = false;
        playerHandle = null;

        if (!pickedUp && interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    private void PickUpShotgun()
    {
        pickedUp = true;

        if (displayShotgun != null)
        {
            displayShotgun.SetActive(false);
        }

        if (playerHandle != null)
        {
            playerHandle.SetActive(true);
        }

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    private static Transform GetPlayerTransform(Collider2D other)
    {
        Transform root = other.transform.root;
        return root.CompareTag("Player") ? root : null;
    }

    private static GameObject FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
