using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.UI;
public class SwitchVCAM : MonoBehaviour
{

    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField]
    private int priorityBoostAmount = 10;
    [SerializeField]
    private Canvas thirdPersonCanvas;
    [SerializeField]
    private Canvas aimCanvas;
    
    private CinemachineVirtualCamera virtualCamera;
    private InputAction aimAction;
    // Start is called before the first frame update
    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        aimAction = playerInput.actions["Aim"];
        aimCanvas.enabled = false;
    }

    private void OnEnable() {
        aimAction.performed += _ => StartAim();
        aimAction.canceled += _ => CancelAim();
    }

    private void OnDisable() {
        aimAction.performed -= _ => StartAim();
        aimAction.canceled -= _ => CancelAim();

    }

    private void OnDestroy() {
        aimAction.performed -= _ => StartAim();
        aimAction.canceled -= _ => CancelAim();
        if (playerInput!=null)
        playerInput.actions.Disable();
    }
    private void StartAim()
    {
        virtualCamera.Priority += priorityBoostAmount;
        if (aimCanvas != null)
            aimCanvas.enabled = true;
        if (thirdPersonCanvas != null)
            thirdPersonCanvas.enabled = false;
    }
    private void CancelAim(){
        virtualCamera.Priority -= priorityBoostAmount;
        if (aimCanvas != null)
            aimCanvas.enabled = false;
        if (thirdPersonCanvas != null)
            thirdPersonCanvas.enabled = true;
    }
}
