using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

public class ComputerCameraSystem : MonoBehaviour
{
    [Header("1. Aþama: Masaya Uçuþ Süresi")]
    public float travelTime = 1.0f;
    public CinemachineCamera playerVirtualCamera;
    public CinemachineCamera computerVirtualCamera;

    [Header("2. Aþama: Ekrana Girme Animasyonu (WTTG)")]
    public float zoomTime = 0.8f;
    public float zoomAmount = 0.4f;

    [Header("LPSP Görselleri (Gizlenecekler)")]
    public GameObject weaponHolder;
    public GameObject lpspCanvasHUD;
    public GameObject armsMesh;

    [Header("GELÝÞMÝÞ KOD YÖNETÝMÝ (BEYAZ LÝSTE)")]
    [Tooltip("Masaya oturduðunuzda AÇIK KALMASINI istediðiniz karakter kodlarý varsa buraya ekleyin. BUNLAR HARÝÇ TÜM OYUNCU KODLARI OTOMATÝK KAPANIR.")]
    public List<MonoBehaviour> allowedPlayerScripts;
    [Tooltip("Masaya oturunca AÇILACAK ekstra bilgisayar kodlarý (Varsa)")]
    public Behaviour[] scriptsToEnableAtComputer;

    [Header("Arayüzler")]
    public GameObject interactionPrompt;
    public GameObject computerScreenCanvas;

    [Header("Ayarlar")]
    public float interactDistance = 3.0f;

    private bool isPlayerNear = false;
    private bool isAtComputer = false;
    private bool isTransitioning = false;
    private Transform playerTransform;
    private Vector3 defaultComputerCamPos;

    // Otomatik kapatýlan kodlarý hafýzada tutar ki kalkýnca geri açabilelim
    private List<MonoBehaviour> automaticallyDisabledScripts = new List<MonoBehaviour>();

    private void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (computerScreenCanvas != null) computerScreenCanvas.SetActive(false);

        if (playerVirtualCamera != null) playerVirtualCamera.Priority = 10;
        if (computerVirtualCamera != null) computerVirtualCamera.Priority = 0;

        if (computerVirtualCamera != null)
        {
            defaultComputerCamPos = computerVirtualCamera.transform.position;
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            bool wasNear = isPlayerNear;
            isPlayerNear = (dist <= interactDistance);

            if (!isAtComputer && interactionPrompt != null)
            {
                if (isPlayerNear && !wasNear) interactionPrompt.SetActive(true);
                else if (!isPlayerNear && wasNear) interactionPrompt.SetActive(false);
            }

            if (!isPlayerNear && isAtComputer) StartCoroutine(TransitionToPlayer());
        }

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (!isAtComputer) StartCoroutine(TransitionToComputer());
            else StartCoroutine(TransitionToPlayer());
        }
    }

    // --- BÝLGÝSAYARA GÝRÝÞ ANÝMASYONU VE TAM DONDURMA ---
    private IEnumerator TransitionToComputer()
    {
        isTransitioning = true;
        isAtComputer = true;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        // 1. Karakterden masaya uçuþ
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null) brain.DefaultBlend.Time = travelTime;
        if (computerVirtualCamera != null) computerVirtualCamera.Priority = 20;

        // Karakter görsellerini gizle
        SetRendererState(weaponHolder, false);
        SetRendererState(armsMesh, false);
        if (lpspCanvasHUD != null) lpspCanvasHUD.SetActive(false);

        // OYUNCUYU TAMAMEN DONDUR (Ateþ etme vs. %100 kapanýr)
        DisableAllPlayerScripts();
        ToggleScripts(scriptsToEnableAtComputer, true);

        // Masaya uçuþu bekle
        yield return new WaitForSeconds(travelTime);

        // 2. Ekranýn içine girme efekti (Zoom)
        float elapsedTime = 0f;
        Vector3 startPos = defaultComputerCamPos;
        Vector3 targetPos = startPos + (computerVirtualCamera.transform.forward * zoomAmount);

        while (elapsedTime < zoomTime)
        {
            computerVirtualCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / zoomTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        computerVirtualCamera.transform.position = targetPos;

        // 3. Ekrana girdik, Canvas'ý aç
        if (computerScreenCanvas != null) computerScreenCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isTransitioning = false;
    }

    // --- MASADAN KALKMA ANÝMASYONU ---
    private IEnumerator TransitionToPlayer()
    {
        isTransitioning = true;
        isAtComputer = false;

        // Önce Canvas'ý kapat
        if (computerScreenCanvas != null) computerScreenCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ekrandan çýkýþ efekti (Zoom Out)
        float elapsedTime = 0f;
        Vector3 startPos = computerVirtualCamera.transform.position;
        Vector3 targetPos = defaultComputerCamPos;

        while (elapsedTime < zoomTime)
        {
            computerVirtualCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / zoomTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        computerVirtualCamera.transform.position = defaultComputerCamPos;

        // Karaktere dönüþ
        if (computerVirtualCamera != null) computerVirtualCamera.Priority = 0;

        // Görselleri aç
        SetRendererState(weaponHolder, true);
        SetRendererState(armsMesh, true);
        if (lpspCanvasHUD != null) lpspCanvasHUD.SetActive(true);

        // OYUNCU KODLARINI ESKÝ HALÝNE GETÝR (Ateþ etme vb. geri açýlýr)
        RestorePlayerScripts();
        ToggleScripts(scriptsToEnableAtComputer, false);

        yield return new WaitForSeconds(travelTime);

        isTransitioning = false;
    }

    // ==========================================
    // YARDIMCI SÝSTEMLER (DOKUNMAYIN)
    // ==========================================

    private void DisableAllPlayerScripts()
    {
        automaticallyDisabledScripts.Clear();

        // Oyuncunun ve altýndaki silahlarýn üzerindeki TÜM kodlarý bul
        MonoBehaviour[] allScripts = playerTransform.GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour script in allScripts)
        {
            if (script == null) continue;

            // Unity'nin temel bileþenlerini (Kamera, Ses, Transform vs.) ve Cinemachine'i BOZMA
            string ns = script.GetType().Namespace;
            if (ns != null && (ns.StartsWith("UnityEngine") || ns.StartsWith("Unity.") || ns.StartsWith("Cinemachine")))
                continue;

            // Eðer kodun açýk kalmasýný özellikle istediysek (Beyaz listede varsa) DOKUNMA
            if (allowedPlayerScripts != null && allowedPlayerScripts.Contains(script))
                continue;

            // Kod çalýþýyorsa, onu acýmasýzca kapat ve hafýzaya al
            if (script.enabled)
            {
                script.enabled = false;
                automaticallyDisabledScripts.Add(script);
            }
        }
    }

    private void RestorePlayerScripts()
    {
        // Masadan kalkýnca hafýzadaki kapalý kodlarý geri aç
        foreach (MonoBehaviour script in automaticallyDisabledScripts)
        {
            if (script != null) script.enabled = true;
        }
        automaticallyDisabledScripts.Clear();
    }

    private void SetRendererState(GameObject obj, bool state)
    {
        if (obj == null) return;
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>()) r.enabled = state;
    }

    private void ToggleScripts(Behaviour[] scripts, bool state)
    {
        foreach (Behaviour s in scripts)
        {
            if (s != null) s.enabled = state;
        }
    }
}