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

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void SetSpriteRotation(Vector2 direction)
    {
        spriteRenderer.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
        spriteRenderer.transform.rotation *= Quaternion.Euler(90, 0, 90);
    }
}
