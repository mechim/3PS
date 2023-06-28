using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammo Pick Up", menuName = "3PS/AmmoPickUp")]
public class AmmoPickUp : ScriptableObject
{
    public string ammoName;
    public int ammoType; // 0 - gun | 1 - cryo | 2 - plasma
    public int ammoAmount;
    public Color lightColor;
    public Material ammoBoxMaterial;
    
    

}
