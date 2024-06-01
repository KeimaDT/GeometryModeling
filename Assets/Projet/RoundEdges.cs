using UnityEngine;

public class RoundEdges : MonoBehaviour
{
    public float minRoundness = 0.1f;
    public float maxRoundness = 0.5f;

    void Start()
    {
        RoundCubeEdges();
    }

    void RoundCubeEdges()
    {
        // Récupère le composant MeshFilter du GameObject
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("Le GameObject ne possède pas de MeshFilter.");
            return;
        }

        // Récupère le mesh du GameObject
        Mesh mesh = meshFilter.mesh;

        // Récupère les vertices du mesh
        Vector3[] vertices = mesh.vertices;

        // Calcule le rayon minimum et maximum pour arrondir les bords
        float minRadius = Mathf.Min(transform.localScale.x, transform.localScale.y, transform.localScale.z) * minRoundness;
        float maxRadius = Mathf.Min(transform.localScale.x, transform.localScale.y, transform.localScale.z) * maxRoundness;

        // Parcourt tous les vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // Calcule un rayon aléatoire pour chaque vertex
            float radius = Random.Range(minRadius, maxRadius);

            // Calcule la distance du vertex à l'origine
            float distance = vertices[i].magnitude;

            // Si la distance est inférieure au rayon, ajuste le vertex pour qu'il soit sur la surface du rayon
            if (distance < radius)
            {
                vertices[i] = vertices[i].normalized * radius;
            }
        }

        // Met à jour les vertices du mesh
        mesh.vertices = vertices;

        // Recalcule les normales et tangentes pour que l'illumination fonctionne correctement
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}