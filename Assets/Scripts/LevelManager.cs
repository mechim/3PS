using UnityEngine;
using TMPro;
public class LevelManager : MonoBehaviour
{
    public int enemiesOnLevel;
    [SerializeField]
    private PlayerController playerController;
    [SerializeField]
    private TMP_Text enemiesLeftText;


    // Update is called once per frame
    void Update()
    {
        if (enemiesOnLevel <= 0){
            playerController.Win();
        }

        enemiesLeftText.text = "Enemies Left: " +enemiesOnLevel.ToString();
    }


}
