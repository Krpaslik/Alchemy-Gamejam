using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class TilemapOneWayPlatforms : MonoBehaviour
{
    [Header("Source")]
    public Tilemap sourceTilemap;
    public TileBase plankTile; // ten Tile asset, kterým maluješ prkna

    [Header("Output")]
    public Transform outputRoot;            // kam se mají edge collidery generovat (child)
    public string outputRootName = "PlankEdges";
    public bool autoRebuildInEditor = true;

    [Header("Collision")]
    public string oneWayLayerName = "OneWayPlatform";

    void Reset()
    {
        sourceTilemap = GetComponent<Tilemap>();
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        if (sourceTilemap == null || plankTile == null)
            return;

        EnsureOutputRoot();
        ClearOutput();

        // Projedeme bounds tilemapy
        var bounds = sourceTilemap.cellBounds;

        // Sesbíráme plank buňky do hashsetu pro rychlé dotazy
        var plankCells = new HashSet<Vector3Int>();
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (sourceTilemap.GetTile(pos) == plankTile)
                plankCells.Add(pos);
        }

        if (plankCells.Count == 0) return;

        // Najdi souvislé horizontální segmenty (v každém y)
        // Segment = řada buněk vedle sebe
        var visited = new HashSet<Vector3Int>();
        int layer = LayerMask.NameToLayer(oneWayLayerName);

        foreach (var cell in plankCells)
        {
            if (visited.Contains(cell)) continue;

            // rozšiř doprava i doleva v rámci stejného y
            int y = cell.y;
            int z = cell.z;

            int xMin = cell.x;
            while (plankCells.Contains(new Vector3Int(xMin - 1, y, z)))
                xMin--;

            int xMax = cell.x;
            while (plankCells.Contains(new Vector3Int(xMax + 1, y, z)))
                xMax++;

            // označ visited
            for (int x = xMin; x <= xMax; x++)
                visited.Add(new Vector3Int(x, y, z));

            // vytvoř EdgeCollider přes HORNÍ hranu segmentu
            var go = new GameObject($"Edge_{y}_{xMin}_{xMax}");
            go.transform.SetParent(outputRoot, false);
            go.layer = layer;

            var edge = go.AddComponent<EdgeCollider2D>();
            edge.isTrigger = false;

            // body v world space: horní levý a horní pravý
            // Pozor: Tilemap má buňku 1×1 unit (pokud máš standardní grid)
            // Horní hrana: (x, y+1)
            Vector3 p0 = sourceTilemap.CellToWorld(new Vector3Int(xMin, y, z));
            Vector3 p1 = sourceTilemap.CellToWorld(new Vector3Int(xMax + 1, y, z));

            // posuň na horní hranu buněk
            p0.y += sourceTilemap.cellSize.y;
            p1.y += sourceTilemap.cellSize.y;

            // EdgeCollider body jsou v lokálním prostoru GO
            // Necháme GO na (0,0) pod outputRoot, takže použijeme world->local
            Vector2 lp0 = outputRoot.InverseTransformPoint(p0);
            Vector2 lp1 = outputRoot.InverseTransformPoint(p1);

            edge.points = new Vector2[] { lp0, lp1 };
        }
    }

    void EnsureOutputRoot()
    {
        if (outputRoot != null) return;

        var existing = transform.Find(outputRootName);
        if (existing != null) outputRoot = existing;
        else
        {
            var go = new GameObject(outputRootName);
            go.transform.SetParent(transform, false);
            outputRoot = go.transform;
        }
    }

    void ClearOutput()
    {
        if (outputRoot == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = outputRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(outputRoot.GetChild(i).gameObject);
            return;
        }
#endif
        for (int i = outputRoot.childCount - 1; i >= 0; i--)
            Destroy(outputRoot.GetChild(i).gameObject);
    }
}
