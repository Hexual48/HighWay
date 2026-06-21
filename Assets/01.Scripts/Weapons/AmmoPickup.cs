using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class AmmoPickup : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject interactButton;
    [SerializeField] private GameObject displayAmmo;

    [Header("Ammo")]
    [SerializeField] private AmmoData ammoToUnlock;
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("Unlock UI")]
    [SerializeField] private UnlockPopupMessage unlockPopup;

    private PlayerAmmo playerAmmo;
    private NewMovement playerMovement;
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

        if (displayAmmo == null)
        {
            Transform ammoTransform = transform.Find("Ammo");
            displayAmmo = ammoTransform != null ? ammoTransform.gameObject : null;
        }

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }

        if (unlockPopup == null)
        {
            unlockPopup = FindFirstObjectByType<UnlockPopupMessage>(FindObjectsInactive.Include);
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
            PickUpAmmo();
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

        playerAmmo = player.GetComponentInChildren<PlayerAmmo>();
        playerMovement = player.GetComponentInChildren<NewMovement>();
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
        playerAmmo = null;
        playerMovement = null;

        if (!pickedUp && interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    private void PickUpAmmo()
    {
        bool wasUnlocked = playerAmmo != null && playerAmmo.IsAmmoUnlocked(ammoToUnlock);

        if (playerAmmo == null || !playerAmmo.UnlockAmmo(ammoToUnlock))
        {
            return;
        }

        pickedUp = true;
        playerMovement?.PlayPickupSound();

        if (!wasUnlocked && unlockPopup != null)
        {
            unlockPopup.Show(ammoToUnlock);
        }

        if (displayAmmo != null)
        {
            displayAmmo.SetActive(false);
        }

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }

    private static Transform GetPlayerTransform(Collider2D other)
    {
        Transform root = other.transform.root;
        return root.CompareTag("Player") ? root : null;
    }
}
