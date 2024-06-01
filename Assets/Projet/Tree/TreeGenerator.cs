using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public GameObject woodPrefab; // Prefab for wood block
    public GameObject leavesPrefab; // Prefab for leaves block
    public Material[] leafMaterials; // Array of materials for leaf colors (Spring, Summer, Autumn, Winter)
    public int trunkHeight = 5; // Height of the trunk
    public int leavesHeight = 5; // Height of the leaves
    public Vector3Int treeSize = new Vector3Int(5, 10, 5); // Overall size of the tree (width, height, depth)

    private Renderer[] leafRenderers;
    private int currentSeasonIndex = 0;

    void Start()
    {
        GenerateCherryTree();
        StartCoroutine(ChangeSeasonCoroutine());
    }

    void GenerateCherryTree()
    {
        Vector3 position = transform.position;

        // Generate the trunk
        for (int y = 0; y < trunkHeight; y++)
        {
            GameObject trunkBlock = Instantiate(woodPrefab, position + new Vector3(0, y, 0), Quaternion.identity, transform);
        }

        // Apply tessellation to the trunk
        for (int i = 0; i < trunkHeight - 1; i++)
        {
            GameObject currentBlock = transform.GetChild(i).gameObject;
            GameObject nextBlock = transform.GetChild(i + 1).gameObject;

            // Calculate the number of subdivisions between current and next block
            int subdivisions = Mathf.RoundToInt(Vector3.Distance(currentBlock.transform.position, nextBlock.transform.position));

            // Apply tessellation by instantiating additional blocks in height and width
            for (int j = 1; j < subdivisions; j++)
            {
                float t = (float)j / subdivisions;
                Vector3 newPosition = Vector3.Lerp(currentBlock.transform.position, nextBlock.transform.position, t);

                // Add width tessellation
                for (int k = -treeSize.x / 2; k <= treeSize.x / 2; k++)
                {
                    Instantiate(woodPrefab, newPosition + new Vector3(k, 0, 0), Quaternion.identity, transform);
                }
            }
        }

        // Generate the leaves with random distribution
        for (int i = 0; i < 1000; i++) // 1000 iterations for random leaf generation
        {
            float x = Random.Range(-treeSize.x / 2f, treeSize.x / 2f);
            float y = Random.Range(trunkHeight, trunkHeight + leavesHeight);
            float z = Random.Range(-treeSize.z / 2f, treeSize.z / 2f);

            if (Random.Range(0f, 1f) > 0.2f) // Randomize a bit for a more natural look
            {
                GameObject leaf = Instantiate(leavesPrefab, position + new Vector3(x, y, z), Quaternion.identity, transform);
                // Store leaf renderer for material change
                if (leafRenderers == null)
                    leafRenderers = new Renderer[1000]; // Assuming maximum 1000 leaves
                leafRenderers[i] = leaf.GetComponent<Renderer>();
            }
        }

        // Apply initial season
        ApplySeasonMaterial();
    }

    IEnumerator ChangeSeasonCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f); // Wait for 5 seconds

            // Change season
            currentSeasonIndex = (currentSeasonIndex + 1) % leafMaterials.Length;
            ApplySeasonMaterial();
        }
    }

    void ApplySeasonMaterial()
    {
        Material seasonMaterial = leafMaterials[currentSeasonIndex];
        
        // Apply season material to all leaf renderers
        foreach (Renderer renderer in leafRenderers)
        {
            if (renderer != null)
            {
                renderer.material = seasonMaterial;
            }
        }
    }
}
