using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject CameraTarget;
    public StarterAssetsInputs inputs;
    public bool isShouldRotate;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraTarget.SetActive(true);
            inputs.cursorLocked = false;
            inputs.cursorInputForLook = false;

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }
}
