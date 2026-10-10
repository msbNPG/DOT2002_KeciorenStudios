using UnityEngine;
using UnityEditor;

public class CreateSphereTool
{
    // Üst menüde "Tools" altına "Create Sphere at 0,0,0" adında bir buton ekler
    [MenuItem("Tools/Create Sphere at 0,0,0")]
    public static void CreateSphereAtOrigin()
    {
        // Sahneye yeni bir küre (PrimitiveType.Sphere) oluşturur
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        // Kürenin ismini belirler
        sphere.name = "OriginSphere";
        
        // Kürenin pozisyonunu tam olarak (0, 0, 0) noktasına ayarlar
        sphere.transform.position = Vector3.zero;
        
        // İşlemi Geri Alınabilir (Undo) yapar, böylece Ctrl+Z ile küreyi silebilirsiniz
        Undo.RegisterCreatedObjectUndo(sphere, "Create Sphere at Origin");
        
        // Hiyerarşide (Hierarchy) yeni oluşturulan küreyi otomatik olarak seçili hale getirir
        Selection.activeGameObject = sphere;
        
        Debug.Log("0,0,0 noktasına yeni bir küre başarıyla oluşturuldu!");
    }
}
