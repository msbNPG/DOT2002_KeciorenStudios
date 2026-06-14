using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Eþya Ayarlarý")]
    public string itemName = "Aþý Malzemesi";

    // Oyuncunun eþyanýn dibinde olup olmadýðýný kontrol eden kilit noktamýz
    private bool oyuncuYakininda = false;

    void Update()
    {
        // Eðer oyuncu eþyanýn yanýndaysa VE klavyeden E tuþuna basarsa (GetKeyDown)
        if (oyuncuYakininda && Input.GetKeyDown(KeyCode.E))
        {
            // GameManager'a haber ver ve sayacý artýr
            GameManager.instance.EsyaToplandi();

            // Ekrana ne aldýðýmýzý yazdýr
            Debug.Log(itemName + " çantaya eklendi!");

            // Eþyayý sahneden sil
            Destroy(gameObject);
        }
    }

    // Oyuncu eþyanýn Is Trigger alanýna girdiðinde
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuYakininda = true;
            Debug.Log(itemName + " almak için 'E' tuþuna bas."); // Oyuncuya ipucu
        }
    }

    // Oyuncu eþyayý almadan uzaklaþýp alandan çýktýðýnda
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuYakininda = false;
        }
    }
}