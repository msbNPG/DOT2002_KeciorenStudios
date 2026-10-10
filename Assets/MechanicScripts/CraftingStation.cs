using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    private bool oyuncuYakininda = false;

    void Update()
    {
        // Oyuncu masanýn yanýndaysa ve E'ye basarsa
        if (oyuncuYakininda && Input.GetKeyDown(KeyCode.E))
        {
            // GameManager üzerinden sayacý kontrol ediyoruz
            if (GameManager.instance.toplananEsya >= GameManager.instance.gerekenEsya)
            {
                // KAZANMA DURUMU
                Debug.Log("TEBRÝKLER! AÞIYI CRAFTLADIN VE DÜNYAYI KURTARDIN!");
                // Birazdan buraya oyunu bitirme kodlarýný ekleyeceðiz
            }
            else
            {
                // EKSÝK EÞYA DURUMU
                int eksik = GameManager.instance.gerekenEsya - GameManager.instance.toplananEsya;
                Debug.Log("Aþýyý tamamlamak için " + eksik + " parçaya daha ihtiyacýn var!");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuYakininda = true;
            Debug.Log("Craft masasýna yaklaþtýn. Aþýyý hazýrlamak için 'E' tuþuna bas.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuYakininda = false;
        }
    }
}