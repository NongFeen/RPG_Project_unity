using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float fadeSpeed = 2f;

    private TextMeshPro text;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        // Move up
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - fadeSpeed * Time.deltaTime);
        if (text.color.a <= 0)
            Destroy(gameObject);
    }
}

