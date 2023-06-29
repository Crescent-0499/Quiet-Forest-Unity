using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// 用于游戏光标控制
public class UIController : MonoBehaviour
{
    private PlayerControls Controls;
    [SerializeField] private GameObject removecam;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button newButton;
    [SerializeField] private Button exitButton;
    bool isPasue;
    public bool CancelAllMovement { get; set;}
    void Start()
    {
        resumeButton.onClick.AddListener(() => OnResumeButtonClicked());
        exitButton.onClick.AddListener(() => OnExitButtonClicked());
        newButton.onClick.AddListener(() => OnNewButtonClicked());
        Controls = new PlayerControls();
        Controls.Enable();
        Controls.Keyboard.Escape.performed += ctx =>
        {
            isPasue = !isPasue;
        };
    }

    private void Update()
    {
        HandleOpenMenu();
    }
    private void HandleOpenMenu()
    {
        if (isPasue)
        { 
            Time.timeScale = 0; // 暂停游戏
            menuUI.SetActive(true);
            removecam.SetActive(false);
            CancelAllMovement = true;
        }
        else
        {
            Time.timeScale = 1; // 继续游戏
            menuUI.SetActive(false);
            removecam.SetActive(true);
            CancelAllMovement = false;
        }
    }
   public void OnResumeButtonClicked()
    {
        isPasue= false;
        Time.timeScale = 1; // 继续游戏
        menuUI.SetActive(false);
        removecam.SetActive(true);
        CancelAllMovement = false;
    }
    
    public void OnNewButtonClicked()
    {
        // 加载初始场景，并保留游戏管理器对象
        SceneManager.LoadScene(2);
    }
    public void OnExitButtonClicked()
    {
        Application.Quit();
        Debug.Log("Quit Application");
    }
}
