using UnityEngine;
using UnityEngine.SceneManagement; // Sahne iþlemleri için gerekli

public class LevelManager : MonoBehaviour
{
    // Butona týklandýðýnda çalýþacak fonksiyon (public olmasý þart!)
    public void RestartLevel()
    {
        // ÇOK ÖNEMLÝ: Yakalandýðýmýzda oyunu durdurmuþtuk (Time.timeScale = 0). 
        // Bölümü yeniden baþlatmadan önce zamaný tekrar normale (1) döndürmeliyiz, yoksa oyun donuk kalýr.
        Time.timeScale = 1f;

        // Mevcut açýk olan sahneyi bul ve en baþtan yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}