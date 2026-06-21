using UnityEngine;

[CreateAssetMenu(fileName = "NewAmmoData", menuName = "Ammo Data")]
public class AmmoData : ScriptableObject
{
    public string ammoName;

    [Header("UI")]
    public Sprite ammoIcon;
    public Sprite ammoInfoIcon;

    [Header("Unlock UI")]
    public Sprite unlockIcon;
    [TextArea]
    public string unlockTitle;
    [TextArea]
    public string unlockDescription;

    public int spreadAngle;
    public int pelletCount;
    public int damagePerPellet;

    public int recoilForce;
    public int penetration;
    public Color color;
}
