using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Color frozenHealthColor1, frozenHealthColor2;
    private LevelManager levelManager;

    private void Awake() {
        currentHealth = maxHealth;
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
    }
    public void TakeDamage(float damage){
        currentHealth -= damage;
        if (currentHealth <= 0){
            Die();
        }
    }

    private void Die(){
        if (gameObject.tag == "Player"){
            var playerController = gameObject.GetComponent<PlayerController>();
            playerController.Die();
        }
        if (gameObject.tag == "HitBox"){
            levelManager.enemiesOnLevel --;
        }
            Destroy(gameObject);
        
            
    }

    public void Freeze(){
        healthSlider.gameObject.transform.Find("Background").GetComponent<Image>().color = frozenHealthColor1;
        healthSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = frozenHealthColor2;
    }
   
    private void Update() {
        //Update UI
        healthSlider.value = currentHealth/100;
    }
}
