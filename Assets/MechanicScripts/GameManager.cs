using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Aþý Craftlama Sistemi")]
    public int toplananEsya = 0;
    public int gerekenEsya = 5;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void EsyaToplandi()
    {
        toplananEsya++;
        Debug.Log("Tedavi Parçasý Toplandý: " + toplananEsya + " / " + gerekenEsya);

        if (toplananEsya >= gerekenEsya)
        {
            Debug.Log("BÜTÜN PARÇALARI TOPLADIM ARTIK AÞIYI ÜRETEBÝLÝRÝM");
        }
    }
}