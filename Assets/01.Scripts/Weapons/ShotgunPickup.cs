using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ShotgunPickup : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject interactButton;
    [SerializeField] private GameObject displayShotgun;
    [SerializeField] private GameObject playerInterface;
    [SerializeField] private GameObject bulletUI;
    [SerializeField] private GameObject speedmeterUI;

    [Header("Unlock UI")]
    [SerializeField] private bool showUnlockPopup;
    [SerializeField] private UnlockPopupMessage unlockPopup;
    [SerializeField] private Sprite unlockIcon;
    [TextArea]
    [SerializeField] private string unlockTitle;
    [TextArea]
    [SerializeField] private string unlockMessage;

    private GameObject playerHandle;
    private ShotgunFire playerShotgunFire;
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

        if (displayShotgun == null)
        {
            Transform shotgunTransform = transform.Find("Shotgun");
            displayShotgun = shotgunTransform != null ? shotgunTransform.gameObject : null;
        }

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }

        if (playerInterface == null)
        {
            GameObject interfaceObject = GameObject.Find("PlayerInterface");
            playerInterface = interfaceObject;
        }

        ResolveWeaponUI();
        SetWeaponUIVisible(false);

        if (unlockPopup == null)
        {
            unlockPopup = FindFirstObjectByType<UnlockPopupMessage>(FindObjectsInactive.Include);
        }

        if (unlockPopup != null)
        {
            unlockPopup.gameObject.SetActive(false);
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
        playerShotgunFire = player.GetComponentInChildren<ShotgunFire>(true);
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
        playerHandle = null;
        playerShotgunFire = null;
        playerMovement = null;

        if (!pickedUp && interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    private void PickUpShotgun()
    {
        pickedUp = true;
        playerMovement?.PlayPickupSound();

        if (displayShotgun != null)
        {
            displayShotgun.SetActive(false);
        }

        if (playerHandle != null)
        {
            playerHandle.SetActive(true);
        }

        if (playerShotgunFire != null)
        {
            playerShotgunFire.enabled = true;
        }

        SetWeaponUIVisible(true);

        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }

        if (showUnlockPopup && unlockPopup != null)
        {
            unlockPopup.Show(unlockIcon, unlockTitle, unlockMessage);
        }
    }

    private void ResolveWeaponUI()
    {
        if (playerInterface == null)
        {
            return;
        }

        if (bulletUI == null)
        {
            Transform bulletTransform = playerInterface.transform.Find("BulletUI");
            bulletUI = bulletTransform != null ? bulletTransform.gameObject : null;
        }

        if (speedmeterUI == null)
        {
            Transform speedmeterTransform = playerInterface.transform.Find("speedmeterUI");
            speedmeterUI = speedmeterTransform != null ? speedmeterTransform.gameObject : null;
        }
    }

    private void SetWeaponUIVisible(bool visible)
    {
        if (bulletUI != null)
        {
            bulletUI.SetActive(visible);
        }

        if (speedmeterUI != null)
        {
            speedmeterUI.SetActive(visible);
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
