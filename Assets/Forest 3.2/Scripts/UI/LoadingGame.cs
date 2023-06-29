using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingGame : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TMPro.TMP_Text loadingText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        startButton.onClick.AddListener(() => OnStartButtonClicked());
        exitButton.onClick.AddListener(() => OnQuitButtonClicked());
        loadingScreen.SetActive(false);
    }

    void LateUpdate()
    {
        if (Cursor.visible == false || Cursor.lockState == CursorLockMode.Locked)
        {
           Cursor.visible = true;
           Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnStartButtonClicked()
    {
        StartCoroutine(LoadGame());
    }
        //异步加载方法
    IEnumerator LoadGame()
    {
        //显示加载画面
        loadingScreen.SetActive(true);

        //异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Forest");

        //阻止场景加载完成后自动切换
        asyncLoad.allowSceneActivation = false;

        //加载进度条
        while (asyncLoad.progress < 0.01f)
        {
            loadingBar.value = Mathf.Clamp01( asyncLoad.progress);
            loadingText.text =  (int)(asyncLoad.progress * 100) + "%";
            yield return null; 
        }

        //加载完成，启动游戏
        asyncLoad.allowSceneActivation = true;
    }
   public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

}