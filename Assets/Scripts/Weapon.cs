using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GunData gunData;
    public Transform barrelTransform;
    public int bulletsLeft;
    public int totalBullets;

    private void Awake() {
        bulletsLeft = gunData.ammoCapacity;
    }
}
