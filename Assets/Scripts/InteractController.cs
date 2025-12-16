using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.white;
    public float highlightIntensity = 2f;

    private Renderer objRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;

    public bool isCloth;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();

        if (objRenderer != null)
        {
            originalMaterial = objRenderer.material;
            highlightMaterial = new Material(originalMaterial);
        }
    }

    void Update()
    {

    }

    // Mouse masuk ke object (hover)
    void OnMouseEnter()
    {
        if (objRenderer != null)
        {
            // Highlight dengan warna putih/terang
            highlightMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);
            highlightMaterial.EnableKeyword("_EMISSION");
            objRenderer.material = highlightMaterial;
        }

        Debug.Log("Mouse hover: " + gameObject.name);
    }

    // Mouse keluar dari object
    void OnMouseExit()
    {
        if (objRenderer != null)
        {
            // Kembalikan material original
            objRenderer.material = originalMaterial;
        }
    }

    // Mouse klik object
    void OnMouseDown()
    {
        Debug.Log("Clicked: " + gameObject.name);
        OnObjectClicked();
    }

    void OnObjectClicked()
    {
        // Logika saat object diklik
        Debug.Log("Object interacted!");

        // Contoh: buka UI, trigger event, dll
    }
}
