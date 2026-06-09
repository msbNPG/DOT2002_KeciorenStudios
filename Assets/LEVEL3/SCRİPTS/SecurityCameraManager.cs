using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public class CameraButtonSettings
{
    [Tooltip("Buton objesini buraya sürükleyin")]
    public RectTransform buttonRect;

    [Tooltip("Kamera moduna geçildiğinde bu buton TAM OLARAK hangi büyüklükte (X, Y, Z) olsun?")]
    public Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);

    [Tooltip("Kamera moduna geçildiğinde bu buton TAM OLARAK hangi koordinata (X, Y) gitsin?")]
    public Vector2 targetPosition = new Vector2(600f, 300f);

    // Orijinal değerler (Kod tarafından otomatik doldurulur)
    [HideInInspector] public Vector3 defaultScale;
    [HideInInspector] public Vector2 defaultPosition;
}

public class SecurityCameraManager : MonoBehaviour
{
    [Header("Güvenlik Kameraları")]
    public CinemachineCamera[] securityCameras;

    [Header("Gizlenecek Arka Plan")]
    public GameObject backgroundPanel;

    [Header("TÜM BUTONLARIN TOPLU AYARLARI")]
    public CameraButtonSettings[] buttonSettings;

    private void Start()
    {
        // 1. Oyun başında tüm butonların "Masadaki" orijinal hallerini kaydet
        foreach (var btn in buttonSettings)
        {
            if (btn.buttonRect != null)
            {
                btn.defaultScale = btn.buttonRect.localScale;
                btn.defaultPosition = btn.buttonRect.anchoredPosition;
            }
        }

        // 2. Başlangıçta her şeyin düzgün (Default) olduğundan emin ol
        SetAllButtonsToDefault();
    }

    // Herhangi bir kamera butonuna basıldığında çalışacak fonksiyon
    public void ViewCamera(int index)
    {
        // KAMERA YÖNETİMİ
        foreach (var cam in securityCameras)
        {
            if (cam != null) cam.Priority = 0;
        }

        if (index >= 0 && index < securityCameras.Length)
        {
            if (securityCameras[index] != null)
            {
                securityCameras[index].Priority = 30;
                if (backgroundPanel != null) backgroundPanel.SetActive(false);
            }
        }

        // BUTON YÖNETİMİ: Herhangi birine basıldıysa HEPSİNİ "Target" konumuna al
        SetAllButtonsToTarget();
    }

    // "Masaya Dön" butonuna basıldığında çalışacak fonksiyon
    public void CloseSecurityCameras()
    {
        // 1. Kameraları sıfırla
        foreach (var cam in securityCameras)
        {
            if (cam != null) cam.Priority = 0;
        }

        // 2. Arka planı geri getir
        if (backgroundPanel != null) backgroundPanel.SetActive(true);

        // 3. BUTON YÖNETİMİ: HEPSİNİ eski orijinal yerlerine geri çek
        SetAllButtonsToDefault();
    }

    // TÜM BUTONLARI HEDEF (AKTİF) KONUMA GETİRİR
    private void SetAllButtonsToTarget()
    {
        foreach (var btn in buttonSettings)
        {
            if (btn.buttonRect != null)
            {
                btn.buttonRect.localScale = btn.targetScale;
                btn.buttonRect.anchoredPosition = btn.targetPosition;
            }
        }
    }

    // TÜM BUTONLARI BAŞLANGIÇ (PASİF) KONUMUNA GETİRİR
    private void SetAllButtonsToDefault()
    {
        foreach (var btn in buttonSettings)
        {
            if (btn.buttonRect != null)
            {
                btn.buttonRect.localScale = btn.defaultScale;
                btn.buttonRect.anchoredPosition = btn.defaultPosition;
            }
        }
    }
}