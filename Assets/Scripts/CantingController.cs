using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CantingController : MonoBehaviour
{
    [Header("Canvas Setup")]
    public RawImage canvasImage; // Tempat gambar
    public RawImage patternReference; // Pola yang harus diikuti (bisa dishow/hide)

    [Header("Pattern")]
    public Texture2D patternTexture; // Pola template
    public float accuracyThreshold = 0.75f; // 75% akurat = pass

    [Header("Brush Settings")]
    public Color brushColor = new Color(0.4f, 0.3f, 0.2f); // Warna lilin batik
    public int brushSize = 5;
    public bool showPattern = true; // Toggle lihat pola atau tidak

    [Header("UI Feedback")]
    public Text accuracyText;
    public GameObject completePanel;

    private Texture2D drawTexture;
    private bool isDrawing = false;
    private Vector2 lastDrawPos;
    private int textureSize = 512;

    void Start()
    {
        InitializeCanvas();

        if (patternReference != null && patternTexture != null)
        {
            patternReference.texture = patternTexture;
            patternReference.gameObject.SetActive(showPattern);
        }
    }

    void InitializeCanvas()
    {
        // Buat texture kosong untuk gambar
        drawTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        // Fill dengan warna putih (kain kosong)
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        drawTexture.SetPixels(pixels);
        drawTexture.Apply();

        canvasImage.texture = drawTexture;
    }

    void Update()
    {
        HandleDrawing();

        // Toggle pattern reference
        if (Input.GetKeyDown(KeyCode.P))
        {
            showPattern = !showPattern;
            if (patternReference != null)
            {
                patternReference.gameObject.SetActive(showPattern);
            }
        }

        // Check accuracy
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CheckAccuracy();
        }
    }

    void HandleDrawing()
    {
        // Mouse/touch input
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasImage.rectTransform,
                Input.mousePosition,
                null,
                out localPos))
            {
                isDrawing = true;
                lastDrawPos = LocalToTextureCoord(localPos);
                DrawAt(lastDrawPos);
            }
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasImage.rectTransform,
                Input.mousePosition,
                null,
                out localPos))
            {
                Vector2 currentPos = LocalToTextureCoord(localPos);
                DrawLine(lastDrawPos, currentPos);
                lastDrawPos = currentPos;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }

    Vector2 LocalToTextureCoord(Vector2 localPos)
    {
        RectTransform rt = canvasImage.rectTransform;

        // Convert local pos ke 0-1 range
        float x = (localPos.x + rt.rect.width / 2) / rt.rect.width;
        float y = (localPos.y + rt.rect.height / 2) / rt.rect.height;

        // Convert ke texture coordinate
        return new Vector2(
            Mathf.Clamp01(x) * textureSize,
            Mathf.Clamp01(y) * textureSize
        );
    }

    void DrawAt(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        // Draw circular brush
        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                if (i * i + j * j <= brushSize * brushSize)
                {
                    int px = Mathf.Clamp(x + i, 0, textureSize - 1);
                    int py = Mathf.Clamp(y + j, 0, textureSize - 1);

                    drawTexture.SetPixel(px, py, brushColor);
                }
            }
        }

        drawTexture.Apply();
    }

    void DrawLine(Vector2 start, Vector2 end)
    {
        // Interpolate untuk garis smooth
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);
            DrawAt(point);
        }
    }

    void CheckAccuracy()
    {
        if (patternTexture == null)
        {
            Debug.LogWarning("No pattern texture assigned!");
            return;
        }

        float accuracy = CalculateAccuracy();

        if (accuracyText != null)
        {
            accuracyText.text = $"Accuracy: {(accuracy * 100):F1}%";
        }

        Debug.Log($"Pattern Accuracy: {accuracy * 100}%");

        if (accuracy >= accuracyThreshold)
        {
            OnPatternComplete();
        }
        else
        {
            Debug.Log($"Not accurate enough. Need {accuracyThreshold * 100}%, got {accuracy * 100}%");
        }
    }

    float CalculateAccuracy()
    {
        // Resize pattern ke ukuran draw texture kalau beda
        Texture2D scaledPattern = patternTexture;
        if (patternTexture.width != textureSize || patternTexture.height != textureSize)
        {
            scaledPattern = ScaleTexture(patternTexture, textureSize, textureSize);
        }

        Color[] patternPixels = scaledPattern.GetPixels();
        Color[] drawnPixels = drawTexture.GetPixels();

        int correctPixels = 0;      // Warnain di area hitam (BENAR)
        int totalPatternPixels = 0; // Total area hitam yang harus diwarnain
        int wrongPixels = 0;        // Warnain di area putih (SALAH/keluar garis)

        // Hitung pixel
        for (int i = 0; i < patternPixels.Length; i++)
        {
            bool isBlackArea = !IsWhitePixel(patternPixels[i]); // Area hitam yang harus diwarnain
            bool playerDrawn = !IsWhitePixel(drawnPixels[i]);   // Player udah gambar di sini

            if (isBlackArea)
            {
                totalPatternPixels++;

                if (playerDrawn)
                {
                    correctPixels++; // Warnain area hitam = BENAR
                }
            }
            else
            {
                // Area putih (jangan diwarnain)
                if (playerDrawn)
                {
                    wrongPixels++; // Warnain area putih = SALAH (keluar garis)
                }
            }
        }

        if (totalPatternPixels == 0) return 0f;

        // Hitung completion (berapa persen area hitam yang udah diwarnain)
        float completion = (float)correctPixels / totalPatternPixels;

        // Hitung penalty (kalau banyak keluar garis)
        float penalty = (float)wrongPixels / totalPatternPixels;

        // Final accuracy = completion - penalty
        // Tapi minimum 0
        float accuracy = Mathf.Max(0f, completion - (penalty * 0.5f)); // Penalty cuma 50% dari wrong pixels

        // Debug info
        Debug.Log($"Correct: {correctPixels}/{totalPatternPixels} ({completion * 100:F1}%)");
        Debug.Log($"Wrong: {wrongPixels} (Penalty: {penalty * 100:F1}%)");
        Debug.Log($"Final Accuracy: {accuracy * 100:F1}%");

        return accuracy;
    }

    bool IsWhitePixel(Color pixel)
    {
        // Threshold untuk detect putih
        return pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f;
    }

    Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight);

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float u = x / (float)targetWidth;
                float v = y / (float)targetHeight;

                Color pixel = source.GetPixelBilinear(u, v);
                result.SetPixel(x, y, pixel);
            }
        }

        result.Apply();
        return result;
    }

    void OnPatternComplete()
    {
        Debug.Log("Pattern completed successfully!");

        if (completePanel != null)
        {
            completePanel.SetActive(true);
        }

        // Trigger reward, next level, etc
    }

    public void ClearCanvas()
    {
        InitializeCanvas();
    }

    public void TogglePatternVisibility()
    {
        showPattern = !showPattern;
        if (patternReference != null)
        {
            patternReference.gameObject.SetActive(showPattern);
        }
    }
}
