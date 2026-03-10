using UnityEngine;

public class EyeScanner : MonoBehaviour
{
    [Header("Tarama Ayarlarý")]
    [Tooltip("Gözün dönme hýzý.")]
    public float scanSpeed = 1.5f;

    [Tooltip("Gözün merkezden saða ve sola maksimum kaç derece döneceði.")]
    public float scanAngle = 60f;

    private Quaternion startRotation;

    void Start()
    {
        // Gözün oyuna baþladýðý ilk açýyý merkez olarak kaydediyoruz
        startRotation = transform.rotation;
    }

    void Update()
    {
        // Sinüs dalgasý kullanarak zamanla deðiþen, yumuþak bir -1 ile 1 arasý deðer alýyoruz
        // Bunu scanAngle ile çarparak açýyý belirliyoruz.
        float currentAngle = Mathf.Sin(Time.time * scanSpeed) * scanAngle;

        // Gözü sadece Y ekseninde (saða-sola) döndürüyoruz
        transform.rotation = startRotation * Quaternion.Euler(0, currentAngle, 0);
    }
}