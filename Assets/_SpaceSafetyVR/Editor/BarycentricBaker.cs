using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BarycentricBaker
{
    [MenuItem("Tools/Bake Barycentrics to UV2")]
    static void Bake()
    {
        var meshFilter = Selection.activeGameObject?.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) return;
        
        

        int[] triangles = mesh.triangles;
        Vector3[] barycentrics = new Vector3[mesh.vertexCount];

        // Reset
        for (int i = 0; i < barycentrics.Length; i++) barycentrics[i] = Vector3.zero;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            barycentrics[i0] += new Vector3(1, 0, 0);
            barycentrics[i0].x = Mathf.Min(barycentrics[i0].x, 2);

            barycentrics[i1] += new Vector3(0, 1, 0);
            barycentrics[i1].y = Mathf.Min(barycentrics[i1].y, 2);

            barycentrics[i2] += new Vector3(0, 0, 1);
            barycentrics[i2].z = Mathf.Min(barycentrics[i2].z, 2);
        }

        mesh.SetUVs(1, new System.Collections.Generic.List<Vector3>(barycentrics));
        
        // Save mesh asset permanently
        Mesh newMesh = Object.Instantiate(mesh);
        string path = "Assets/BakedMeshes/" + mesh.name + "_Bary.asset";
        AssetDatabase.CreateAsset(newMesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Barycentrics baked to " + path);
    }
}