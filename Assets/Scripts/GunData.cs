using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName ="GunData",menuName = "3PS/GunData")]
public class GunData : ScriptableObject
{



    [Header("Properties")]
    public string gunName;
    public int damage;
    public int ammoCapacity;
    public float timeBetweenShots;
    public float reloadTime;
    public bool canHold;
    public Color color;


    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlash;
    public GameObject impactEffect;

}
