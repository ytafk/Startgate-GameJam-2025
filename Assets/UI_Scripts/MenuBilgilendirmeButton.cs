using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuBilgilendirmeButtonsc : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Geçilecek Sahne Adý")]
    public string sceneName;

    [Header("Animasyon Ayarlarý")]
    public Animator animator;          // Animator referansý
    public Image image;                // Duraðan sprite için UI Image
    public Sprite defaultSprite;       // Normal sprite
    public string hoverAnimationName;  // Oynatýlacak animasyonun adý

    private Button btn;

    void Start()
    {
        // Button komponentini al
        btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(LoadScene);
        }
        else
        {
            Debug.LogWarning("Button komponenti bulunamadý!");
        }

        // Baþlangýçta animasyonu kapatýp default sprite göster
        if (animator != null) animator.enabled = false;
        if (image != null && defaultSprite != null) image.sprite = defaultSprite;
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Sahne adý boþ!");
        }
    }

    // Mouse üzerine geldiðinde
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play(hoverAnimationName, -1, 0f);
        }
    }

    // Mouse ayrýldýðýnda
    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null) animator.enabled = false;
        if (image != null && defaultSprite != null) image.sprite = defaultSprite;
    }
}
