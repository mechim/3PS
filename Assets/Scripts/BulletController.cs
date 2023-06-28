using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletDecal;
    [SerializeField]
    private float speed = 50f;
    [SerializeField]
    private float timeToDestroy = 3f;
    [SerializeField]
    private float damage;

    private Transform enemyTransform;

    public Vector3 directionSnapshot;
    private bool directionSet;
    private void Awake() {
        
    }
    private void OnEnable() {
        Destroy(gameObject, timeToDestroy);
       
    }

    private void Update() {        
        transform.Translate(directionSnapshot * speed *Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter(Collision other) {
        ContactPoint contact = other.GetContact(0);
         
        if (other.collider.gameObject.tag == "Player"){
            var healthManager = other.gameObject.GetComponent<HealthManager>();

            healthManager.TakeDamage(damage);
        }
        if (other.collider.gameObject.tag != "Enemy") Destroy(gameObject);

    }
}
