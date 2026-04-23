using UnityEngine;
using UnityEngine.UI;

public class MenuScroll : MonoBehaviour
{
    [SerializeField] Vector2 scrollVectorTop;
    [SerializeField] Vector2 scrollVectorBottom;
    [SerializeField] public Scrollbar ScrollBar;
    [SerializeField] public GameObject ScrollBarObject;
    private void Start()
    {
        scrollVectorTop = transform.position;
        scrollVectorBottom = transform.position;
        scrollVectorBottom.y = -scrollVectorTop.y;
        ScrollBar = ScrollBarObject.GetComponent<Scrollbar>();
    }
    public void OnScroll()
    {
        Vector2 scrollVector = Vector2.Lerp(scrollVectorTop, scrollVectorBottom, ScrollBar.value);
        gameObject.transform.position = scrollVector;

    }
}
