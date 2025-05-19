using UnityEngine;

public class GridSystemDebugSingle : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public void Setup(int x, int y, float gridNodeSize)
    {
        transform.position = GridSystem.GetWorldPosition(x, y, gridNodeSize);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }
}
