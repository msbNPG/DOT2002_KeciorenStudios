using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    [Header("Crafting (Asi Uretim) Merkezi")]
    public string stationName = "Asi Istasyonu";

    /// <summary>
    /// Oyuncu tarafindan Raycast (E tusu) ile tetiklenir.
    /// PlayerInteract scriptinden cagirilir.
    /// </summary>
    public void Interact()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("[CraftingStation] GameManager bulunamadi!");
            return;
        }

                // Toplanan parca sayisini kontrol et
        if (GameManager.instance.toplananEsya >= GameManager.instance.gerekenEsya)
        {
            // KAZANMA DURUMU
            string mesaj = "TEBRİKLER! AŞIYI ÜRETTİN!";
            Debug.Log(mesaj);
            GameManager.instance.ShowPopupText(mesaj, Color.green);
            
            // GameManager uzerinden Win mantigini tetikle
            GameManager.instance.GameWon();
        }
        else
        {
            // EKSİK PARCA DURUMU
            int eksik = GameManager.instance.gerekenEsya - GameManager.instance.toplananEsya;
            string mesaj = "Aşıyı tamamlamak için " + eksik + " parça daha lazım";
            Debug.Log(mesaj);
            
            // Ekranda goster (Kirmizi renk ile uyari)
            GameManager.instance.ShowPopupText(mesaj, Color.red);
        }
    }
}