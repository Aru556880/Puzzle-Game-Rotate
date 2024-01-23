using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelBuilder : MonoBehaviour
{
    public float GridSize
    {
        get { return GetWorldFromGrid(new Vector2Int(1,0)).x - GetWorldFromGrid(new Vector2Int(0,0)).x; }
    }
    public GameObject AllActors;
    [SerializeField] Grid grid;
    [SerializeField] Tilemap tilemap;
    
    public Vector2Int GetGridFromWorld(Vector2 worldPos)
    {
        return (Vector2Int)tilemap.WorldToCell(worldPos);
    }
    public Vector2 GetWorldFromGrid(Vector2Int gridPos)
    {
        return tilemap.GetCellCenterWorld((Vector3Int)gridPos);
    }
    void Start()
    {
        Initialization();
    }

    void Initialization()
    {
        foreach(Transform child in AllActors.transform)
        {
            Vector2Int gridPosition = GetGridFromWorld(child.transform.position);
            child.transform.position = GetWorldFromGrid(gridPosition);
        }
    }
    public bool IsWall(Vector2 worldPos)
    {
        Vector2Int gridPos = GetGridFromWorld(worldPos);
        return tilemap.GetTile((Vector3Int)gridPos) != null;
    }
    public TileBase GetTileAt(Vector2 worldPos)
    {
        Vector2Int gridPos = GetGridFromWorld(worldPos);
        return tilemap.GetTile((Vector3Int)gridPos);
    }
}
