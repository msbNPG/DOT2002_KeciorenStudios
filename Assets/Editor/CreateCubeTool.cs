using UnityEngine;
using UnityEditor;

public class CreateCubeTool
{
    // Üst menüde "Tools" altına "Create Cube at 0,0,0" adında bir buton ekler
    [MenuItem("Tools/Create Cube at 0,0,0")]
    public static void CreateCubeAtOrigin()
    {
        // Sahneye yeni bir küp (PrimitiveType.Cube) oluşturur
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        // Küpün ismini belirler
        cube.name = "OriginCube";
        
        // Küpün pozisyonunu tam olarak (0, 0, 0) noktasına ayarlar
        cube.transform.position = Vector3.zero;
        
        // İşlemi Geri Alınabilir (Undo) yapar, böylece Ctrl+Z ile küpü silebilirsiniz
        Undo.RegisterCreatedObjectUndo(cube, "Create Cube at Origin");
        
        // Hiyerarşide (Hierarchy) yeni oluşturulan küpü otomatik olarak seçili hale getirir
        Selection.activeGameObject = cube;
        
        Debug.Log("0,0,0 noktasına yeni bir küp başarıyla oluşturuldu!");
    }
}
