using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    static public LevelManager Instance;
    [SerializeField] Canvas levelManagerCanvas;
    [SerializeField] Image maskImage;
    private void Awake() 
    {
        if(Instance!=null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start() 
    {
        maskImage.gameObject.SetActive(false);
    }
    public void EnterLevelTest()
    {
        StartCoroutine(LoadSceneCoroutine("Level1-2"));
    }
    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return StartCoroutine(HideSceneCoroutine());
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        yield return StartCoroutine(RevealSceneCoroutine(asyncOperation));
    }
    IEnumerator HideSceneCoroutine()
    {
        maskImage.gameObject.gameObject.SetActive(true);
        maskImage.rectTransform.sizeDelta = Vector2.zero;

        float progress = 0;
        while(progress < 1)
        {
            maskImage.rectTransform.sizeDelta = new Vector2(5000,5000) * (1-progress);
            progress += Time.deltaTime * 1.5f;

            if(progress>1) maskImage.rectTransform.sizeDelta = Vector2.zero;
            yield return null;
        }
    }
    IEnumerator RevealSceneCoroutine(AsyncOperation asyncOperation)
    {
        maskImage.gameObject.gameObject.SetActive(true);
        maskImage.rectTransform.sizeDelta = Vector2.zero;

        yield return new WaitForSeconds(1f);
        while(!asyncOperation.isDone) yield return null;

        Player player = FindAnyObjectByType<Player>();
        
        float progress = 0;
        while(progress < 1)
        {
            maskImage.rectTransform.sizeDelta = new Vector2(5000,5000) * progress;
            progress += Time.deltaTime * 1.5f;
            yield return null;
        }
        maskImage.gameObject.gameObject.SetActive(false);
        if(Player.Instance!=null) Player.Instance.CanPlayerControl = true;
    }
}
