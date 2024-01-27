using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    static public LevelManager levelManager;
    [SerializeField] Canvas levelManagerCanvas;
    [SerializeField] Image maskImage;
    private void Awake() 
    {
        if(levelManager!=null)
        {
            Destroy(gameObject);
        }
        else
        {
            levelManager = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void EnterLevelTest()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Level1-1");
        StartCoroutine(RevealSceneCoroutine(asyncOperation));
    }
    IEnumerator RevealSceneCoroutine(AsyncOperation asyncOperation)
    {
        levelManagerCanvas.gameObject.SetActive(true);
        maskImage.rectTransform.sizeDelta = Vector2.zero;
        while(!asyncOperation.isDone) yield return null;

        Player player = FindAnyObjectByType<Player>();
        levelManagerCanvas.gameObject.SetActive(true);
        
        float progress = 0;
        while(progress < 1)
        {
            maskImage.rectTransform.sizeDelta = new Vector2(5000,5000) * progress;
            progress += Time.deltaTime * 1.5f;
            yield return null;
        }
    }
}
