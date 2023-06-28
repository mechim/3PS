using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float defaultSpeed = 2.0f;
    [SerializeField]
    private float sprintSpeed = 4.0f;
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float rotationSpeed = 5f;
    [Header("Shooting")]
    public Weapon weapon;
    [SerializeField]
    private GameObject bulletDecal;


    [Header("Weapon Switching")]
    [SerializeField]
    private Transform weaponParent;
    [SerializeField]
    private GameObject gunObject, cryoObject, plasmaObject;
    private Weapon gun, cryo, plasma;
    private int selectedWeapon = 0;
    [Header("UI")]
    [SerializeField]
    private TMP_Text ammoText, weaponText;

    [SerializeField]
    private Canvas hudCanvas, endCanvas, thirdPersonCanvas, aimCanvas, pauseCanvas; 
    


    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private bool isSprinting;
    private PlayerInput playerInput;
    private InputAction shootAction;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction reloadAction;
    private InputAction sprintAction;
    private InputAction chooseGunAction;
    private InputAction chooseCryoAction;
    private InputAction choosePlasmaAction;
    private InputAction pauseAction;
    private Transform cameraTransform;

    private bool readyToShoot;
    private bool reloading;
    private bool isAiming;
    private ParticleSystem muzzle;


    //Animations
    private Animator animator;
    private int moveXAnimationParameterID, moveZAnimationParameterID, jumpAnimation, recoilAnimation;
    private Vector2 currentAnimationBlend;
    private Vector2 animationVelocity;
    private float animationSmoothTime = .1f;
    private float animationPlayTransition = .15f;

    [Header("Rigging")]
    [SerializeField]
    private Transform aimTarget;
    [SerializeField]
    private float aimDistance = 10;
    [Header("Stats")]
    private StatsManager statsManager;
    private bool statsTriggered;
    [SerializeField]
    private TMP_Text winStatsText;
    [SerializeField]
    private TMP_Text lostStatsText;
    [SerializeField]
    private TMP_Text pauseStatsText;
    private bool isPaused = false;
    [SerializeField]
    private AudioSource audioSource;
    private void Awake()
    {
        Resume();
        //Lock the Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
        controller = gameObject.GetComponent<CharacterController>();
        playerInput = gameObject.GetComponent<PlayerInput>();
        //Configure Player Input Actions
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];
        reloadAction = playerInput.actions["Reload"];
        sprintAction = playerInput.actions["Sprint"];

        chooseGunAction = playerInput.actions["ChooseGun"];
        chooseCryoAction = playerInput.actions["ChooseCryo"];
        choosePlasmaAction = playerInput.actions["ChoosePlasma"];

        pauseAction = playerInput.actions["Pause"];

        cameraTransform = Camera.main.transform;
        //Configure Weapons
        gun = gunObject.GetComponent<Weapon>();
        cryo = cryoObject.GetComponent<Weapon>();
        plasma = plasmaObject.GetComponent<Weapon>();

        ResetShot();
        
        //Animations

        animator = GetComponent<Animator>();
        moveXAnimationParameterID = Animator.StringToHash("MoveX");
        moveZAnimationParameterID = Animator.StringToHash("MoveZ");
        jumpAnimation = Animator.StringToHash("Rifle Jump");
        recoilAnimation = Animator.StringToHash("Rifle Recoil");
    }

    private void OnEnable() {
        playerInput.actions.Enable();
        shootAction.performed += _ => OnShoot();
        reloadAction.performed += _ => OnReload();
        sprintAction.performed += _ => OnSprint();
        sprintAction.canceled += _ => StopSprint();
        chooseGunAction.performed += _ => OnChooseWeapon(0);
        chooseCryoAction.performed += _ => OnChooseWeapon(1);
        choosePlasmaAction.performed += _ => OnChooseWeapon(2);
        pauseAction.performed += _ => OnPauseAction();
    }

    private void OnDisable() {
        reloadAction.performed -= _ => OnReload();
        reloadAction.canceled -= _ => OnReload();
        shootAction.performed -= _ => OnShoot();
        shootAction.canceled -= _ => OnShoot();
        sprintAction.performed -= _ => OnSprint();
        sprintAction.canceled -= _ => StopSprint();
        chooseGunAction.performed -= _ => OnChooseWeapon(0);
        chooseCryoAction.performed -= _ => OnChooseWeapon(1);
        choosePlasmaAction.performed -= _ => OnChooseWeapon(2);
        pauseAction.performed += _ => OnPauseAction();
        pauseAction.canceled += _ => OnPauseAction();
        playerInput.actions.Disable();
    }

    private void OnPauseAction(){
        if (!isPaused){
            Pause();
        } else {
            Resume();
        }
        
    }

    private void Pause(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        if (pauseCanvas != null){
            pauseCanvas.transform.gameObject.SetActive(true);
            pauseCanvas.enabled = true;
        }
        pauseStatsText.text = "Wins: " + statsManager.gamesWon.ToString() + " Losses: " + statsManager.gamesLost.ToString();
        isPaused = true;
        audioSource.volume = .15f;
    }

    private void Resume(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        if (pauseCanvas != null){
            pauseCanvas.transform.gameObject.SetActive(false);
            pauseCanvas.enabled = false;
        }
        audioSource.volume = .25f;
        isPaused = false;
    }
    
    private void Start() {
        Load();
    }
    private void OnShoot(){
        if (readyToShoot && !reloading && weapon.bulletsLeft > 0){
            Shoot();
        }
    }

    private void OnReload(){
        if (weapon.bulletsLeft < weapon.gunData.ammoCapacity && weapon.totalBullets > 0){
            reloading = true;
            if (this != null)
                Invoke("ReloadFinished", weapon.gunData.reloadTime);
        }
    }
    private void ReloadFinished(){
        var bulletsToRefill = weapon.gunData.ammoCapacity - weapon.bulletsLeft;
        if (weapon.totalBullets <= bulletsToRefill){
            weapon.bulletsLeft += weapon.totalBullets;
            weapon.totalBullets = 0;
        } else if (weapon.totalBullets > bulletsToRefill){
            weapon.bulletsLeft += bulletsToRefill;
            weapon.totalBullets -= bulletsToRefill;
        }
        reloading = false;
    }

    private void OnSprint(){
        if (!isAiming){
            isSprinting = true;
        }   
    }
    private void StopSprint(){
        isSprinting = false;
    }
    private void OnChooseWeapon(int newWeaponIndex){
        if (newWeaponIndex != selectedWeapon){
            switch (newWeaponIndex){
                case 0:
                    weapon = gun;
                    selectedWeapon = 0;
                    break;
                case 1:
                    weapon = cryo;
                    selectedWeapon = 1;
                    break;
                case 2:
                    weapon = plasma;
                    selectedWeapon = 2;
                    break;
            }

            int i = 0;
            if (weaponParent != null)
                foreach (Transform weaponChild in weaponParent){
                    if (weaponChild != null){}
                        if (i == selectedWeapon){
                            weaponChild.gameObject.SetActive(true);
                        } else {
                            weaponChild.gameObject.SetActive(false);
                        }
                        i++;
                }
        }
    }

    private void Shoot(){
        readyToShoot = false;
        weapon.bulletsLeft --;
        if (this != null)
            Invoke("ResetShot", weapon.gunData.timeBetweenShots);
        RaycastHit hit;
        if (animator != null)
            animator.CrossFade(recoilAnimation, animationPlayTransition);

        if (weapon != null){
            muzzle = weapon.barrelTransform.GetChild(0).GetComponent<ParticleSystem>();
            muzzle.Play();
        }
        
        if (cameraTransform != null)
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity)){
                if (hit.transform.tag == "HitBox"){
                    HealthManager healthManager = hit.transform.GetComponent<HealthManager>();
                    healthManager.TakeDamage(weapon.gunData.damage);
                    if(weapon.gunData.name == "CryoGun"){
                        var enemy = hit.transform.GetComponent<EnemyAI>();
                        enemy.Freeze();
                    }
                }

                var impact = Instantiate(weapon.gunData.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
    }

    private void ResetShot(){
        readyToShoot = true;
    }

    void Update()
    {
        //Rigging aim for chest and head
        aimTarget.position = cameraTransform.position + cameraTransform.forward* aimDistance;

        if (aimCanvas.enabled){
            isAiming = true;
            isSprinting = false;
        } else {
            isAiming = false;
        }

        //Keeping the player grounded
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

       
        //Movement
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector2 newInput = new Vector2();
       
         if (!isSprinting){
            playerSpeed = defaultSpeed;
            newInput = new Vector2(input.x/2, input.y/2);
        } else {
            playerSpeed = sprintSpeed;
            newInput = input;
        }
        currentAnimationBlend = Vector2.SmoothDamp(currentAnimationBlend, newInput, ref animationVelocity, animationSmoothTime);
        
        Vector3 move = new Vector3(currentAnimationBlend.x, 0, currentAnimationBlend.y);

        

       

        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f; 
        controller.Move(move * Time.deltaTime * playerSpeed);

        //Animation Blend Tree
        animator.SetFloat(moveXAnimationParameterID, currentAnimationBlend.x);
        animator.SetFloat(moveZAnimationParameterID, currentAnimationBlend.y);


        // Jump
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.CrossFade(jumpAnimation, animationPlayTransition);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        //Rotate mesh towards camrea direction
        float targetAngle = cameraTransform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, targetAngle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed*Time.deltaTime);

        //UI
        UpdateUI();
    }

    private void UpdateUI(){
        ammoText.text = weapon.bulletsLeft.ToString() + " / " + weapon.totalBullets.ToString();
        weaponText.text = weapon.gunData.gunName;
        weaponText.color = weapon.gunData.color;
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Ammo"){
            var ammoObject = other.GetComponent<AmmoLoader>().ammoPickUp;
            switch (ammoObject.ammoType){
                case 0:
                    gun.totalBullets += ammoObject.ammoAmount;
                    break;
                case 1:
                    cryo.totalBullets += ammoObject.ammoAmount;
                    break;
                case 2:
                    plasma.totalBullets += ammoObject.ammoAmount;
                    break;
            }
        }
    }

    public void EndLevel(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        aimCanvas.enabled = false;
        hudCanvas.enabled = false;
        thirdPersonCanvas.enabled = false;

        endCanvas.enabled = true;
        endCanvas.gameObject.SetActive(true);

    }

    public void Die(){
        EndLevel();
        endCanvas.transform.GetChild(0).gameObject.SetActive(true);
        if (!statsTriggered){
            statsManager.gamesLost++;
            statsTriggered = true;
            winStatsText.text = lostStatsText.text = "Wins: " + statsManager.gamesWon.ToString() + " Losses: " + statsManager.gamesLost.ToString();
            Save();
        }
    }

    public void Win(){
        EndLevel();
        endCanvas.transform.GetChild(1).gameObject.SetActive(true);
        if (!statsTriggered){
            statsManager.gamesWon ++;
            statsTriggered = true;
            winStatsText.text = lostStatsText.text = "Wins: " + statsManager.gamesWon.ToString() + " Losses: " + statsManager.gamesLost.ToString();
            Save();
        }
    }

    private void Save(){
        SaveSystem.SaveStats(statsManager);
    }

    private void Load(){
        statsManager = SaveSystem.LoadStats();
    }
}