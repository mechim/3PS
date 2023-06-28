using UnityEngine;
using TMPro;

public class AmmoLoader : MonoBehaviour
{
   public AmmoPickUp ammoPickUp;
   private Renderer objectRenderer;
   private Light hilight;
   [SerializeField]
   private TMP_Text ammoText;

   private void Awake(){
        objectRenderer = GetComponent<Renderer>();
        hilight = transform.GetChild(0).GetComponent<Light>();
        LoadAmmo();

   }

   private void LoadAmmo(){
        hilight.color = ammoPickUp.lightColor;
        objectRenderer.material = ammoPickUp.ammoBoxMaterial;
        ammoText.text = ammoPickUp.ammoName;
        ammoText.color = ammoPickUp.lightColor;
   }

   private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player"){
            Object.Destroy(gameObject);
        }
   }

}
