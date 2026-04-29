using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    [Tooltip("Duvarýn hangi yöne ve ne kadar uzaða gideceði (Örn: X ekseninde 5 birim için X'e 5 yazýn)")]
    public Vector3 moveOffset;

    [Tooltip("Duvarýn gidiþ-dönüþ hýzý")]
    public float speed = 1f;

    private Vector3 startPosition;

    void Start()
    {
        // Oyun baþladýðýnda duvarýn ilk konumunu (A noktasýný) kaydediyoruz
        startPosition = transform.position;
    }

    void Update()
    {
        // Mathf.PingPong, zaman ilerledikçe 0 ile 1 arasýnda sürekli gidip gelen bir deðer (asansör gibi) üretir
        float pingPongValue = Mathf.PingPong(Time.time * speed, 1f);

        // Duvarý, baþlangýç noktasý ile hedef noktasý arasýnda bu deðere göre yumuþakça hareket ettiriyoruz
        transform.position = Vector3.Lerp(startPosition, startPosition + moveOffset, pingPongValue);
    }
}