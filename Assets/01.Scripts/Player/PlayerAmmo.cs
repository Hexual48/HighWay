using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAmmo : MonoBehaviour
{
    [System.Serializable]
    private class AmmoSlot
    {
        public AmmoData ammoData;
        public bool unlockedByDefault;
    }

    [Header("Ammo List")]
    [SerializeField] private List<AmmoSlot> ammoSlots = new List<AmmoSlot>();
    [SerializeField] private int currentAmmoIndex;

    [Header("Debug")]
    [SerializeField] private bool debugLogSelection = true;

    private readonly HashSet<AmmoData> unlockedAmmos = new HashSet<AmmoData>();

    public AmmoData CurrentAmmo => IsUnlockedAmmoIndex(currentAmmoIndex) ? ammoSlots[currentAmmoIndex].ammoData : null;
    public int CurrentAmmoIndex => currentAmmoIndex;
    public event Action AmmoChanged;
    public event Action<int> AmmoSwitched;

    private void Awake()
    {
        InitializeUnlockedAmmos();
        SelectFirstAvailableAmmo();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            SelectPreviousAmmo();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SelectNextAmmo();
        }

        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            DebugCurrentAmmo();
        }
    }

    public bool UnlockAmmo(AmmoData ammoData)
    {
        if (ammoData == null || !ContainsAmmo(ammoData))
        {
            return false;
        }

        bool unlocked = unlockedAmmos.Add(ammoData);

        if (CurrentAmmo == null)
        {
            SelectAmmo(ammoData);
        }

        if (unlocked && debugLogSelection)
        {
            Debug.Log($"Ammo unlocked: {GetAmmoName(ammoData)}", this);
        }

        if (unlocked)
        {
            AmmoChanged?.Invoke();
        }

        return true;
    }

    public bool IsAmmoUnlocked(AmmoData ammoData)
    {
        return ammoData != null && unlockedAmmos.Contains(ammoData);
    }

    public bool CanSelectPreviousAmmo()
    {
        for (int i = currentAmmoIndex - 1; i >= 0; i--)
        {
            if (IsUnlockedAmmoIndex(i))
            {
                return true;
            }
        }

        return false;
    }

    public bool CanSelectNextAmmo()
    {
        for (int i = currentAmmoIndex + 1; i < ammoSlots.Count; i++)
        {
            if (IsUnlockedAmmoIndex(i))
            {
                return true;
            }
        }

        return false;
    }

    public void SelectPreviousAmmo()
    {
        for (int i = currentAmmoIndex - 1; i >= 0; i--)
        {
            if (IsUnlockedAmmoIndex(i))
            {
                SelectAmmoIndex(i);
                AmmoSwitched?.Invoke(-1);
                return;
            }
        }
    }

    public void SelectNextAmmo()
    {
        for (int i = currentAmmoIndex + 1; i < ammoSlots.Count; i++)
        {
            if (IsUnlockedAmmoIndex(i))
            {
                SelectAmmoIndex(i);
                AmmoSwitched?.Invoke(1);
                return;
            }
        }
    }

    private void InitializeUnlockedAmmos()
    {
        unlockedAmmos.Clear();

        for (int i = 0; i < ammoSlots.Count; i++)
        {
            AmmoSlot slot = ammoSlots[i];

            if (slot != null && slot.unlockedByDefault && slot.ammoData != null)
            {
                unlockedAmmos.Add(slot.ammoData);
            }
        }
    }

    private void SelectFirstAvailableAmmo()
    {
        if (IsUnlockedAmmoIndex(currentAmmoIndex))
        {
            SelectAmmoIndex(currentAmmoIndex);
            return;
        }

        for (int i = 0; i < ammoSlots.Count; i++)
        {
            if (IsUnlockedAmmoIndex(i))
            {
                SelectAmmoIndex(i);
                return;
            }
        }
    }

    private void SelectAmmo(AmmoData ammoData)
    {
        for (int i = 0; i < ammoSlots.Count; i++)
        {
            if (ammoSlots[i] != null && ammoSlots[i].ammoData == ammoData)
            {
                SelectAmmoIndex(i);
                return;
            }
        }
    }

    private void SelectAmmoIndex(int index)
    {
        if (!IsUnlockedAmmoIndex(index))
        {
            return;
        }

        currentAmmoIndex = index;
        AmmoChanged?.Invoke();

        if (debugLogSelection)
        {
            DebugCurrentAmmo();
        }
    }

    private bool ContainsAmmo(AmmoData ammoData)
    {
        for (int i = 0; i < ammoSlots.Count; i++)
        {
            if (ammoSlots[i] != null && ammoSlots[i].ammoData == ammoData)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsUnlockedAmmoIndex(int index)
    {
        return IsValidAmmoIndex(index) && unlockedAmmos.Contains(ammoSlots[index].ammoData);
    }

    private bool IsValidAmmoIndex(int index)
    {
        return index >= 0
            && index < ammoSlots.Count
            && ammoSlots[index] != null
            && ammoSlots[index].ammoData != null;
    }

    private void DebugCurrentAmmo()
    {
        AmmoData ammoData = CurrentAmmo;
        string ammoName = ammoData != null ? GetAmmoName(ammoData) : "None";
        Debug.Log($"Current ammo [{currentAmmoIndex}]: {ammoName}", this);
    }

    private static string GetAmmoName(AmmoData ammoData)
    {
        return string.IsNullOrEmpty(ammoData.ammoName) ? ammoData.name : ammoData.ammoName;
    }
}
