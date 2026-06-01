using UnityEngine;

[CreateAssetMenu(fileName = "NewAmmoData", menuName = "Ammo Data")]
public class AmmoData : ScriptableObject
{
    public string ammoName;

    public int spreadAngle;
    public int pelletCount;
    public int damagePerPellet;

    public int recoilForce;
    public int penetration;
    public Color color;
}