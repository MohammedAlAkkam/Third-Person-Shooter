using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.Animations.Rigging;
using UnityEngine.Networking;
public class ThirdPersonShooterContraller : MonoBehaviour
{
    [SerializeField] private GameObject gun;
    [SerializeField] private RigBuilder Chest;
    [SerializeField] public CinemachineVirtualCamera shootingVirtualcamera;
    [SerializeField] public CinemachineVirtualCamera aimVirtualcamera;
    [SerializeField] private float NormalSensitivity;
    [SerializeField] private float AimSensitivity;
    [SerializeField] private Transform target;
    [SerializeField]private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField]private Animator animator;
    [SerializeField]private StarterAssetsInputs starterAssetsInputs;
    [SerializeField] private ThirdPersonController thirdPersonController;
   

    // Start is called before the first frame update
    void Start()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        target = GameObject.Find("Target").transform;
        GetComponentInChildren<MultiAimConstraint>().data.sourceObjects = new WeightedTransformArray{new WeightedTransform( target ,1)};
        Chest.Build();
        
    }


    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999, aimColliderLayerMask))
        {
            target.position = raycastHit.point;
            mouseWorldPos = raycastHit.point;
        }

        if (starterAssetsInputs.Fire)
        {
            Chest.layers[0].active = true;
            shootingVirtualcamera.gameObject.SetActive(true);
            // thirdPersonController.SetSensitivity(AimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            Vector3 worldAimTarget = mouseWorldPos;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDir = (worldAimTarget - transform.forward).normalized;
            //if (!(Camera.main.transform.rotation.eulerAngles.x > 40 || Camera.main.transform.rotation.eulerAngles.x < -10))
            //    transform.forward = Vector3.Lerp(transform.forward, aimDir, .5f);
            transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            if (starterAssetsInputs.Aim) aimVirtualcamera.gameObject.SetActive(true);
            else aimVirtualcamera.gameObject.SetActive(false);
        }
        else
        {
            Chest.layers[0].active = false;
            shootingVirtualcamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(NormalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            //  transform.rotation = Quaternion.Euler(0, Camera.main.transform.transform.eulerAngles.y, 0);

        }
        animator.SetBool("Fire", starterAssetsInputs.Fire);
        
    }

    public void StartDance()
    {
        gun.SetActive(false);
        animator.SetLayerWeight(1, 0);
        thirdPersonController.canMove = false;
    }

    public void EndDance()
    {
        gun.SetActive(true);
        
        animator.SetLayerWeight(1, 1);
        thirdPersonController.canMove = true;
    }



}