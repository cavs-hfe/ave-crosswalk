using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using VRStandardAssets.Utils;

public class LobbyScript : MonoBehaviour
{
    public GameObject targetOne;
    public GameObject targetTwo;

    private GUIArrows guiArrows;

    private Image targetOneImage;
    private Image targetTwoImage;

    private bool targetOneFadedOut = true;
    private bool targetTwoFadedOut = true;

    private int currentTarget = 1;
    private AsyncOperation loadScene = null;

    // Use this for initialization
    void Start()
    {
        guiArrows = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<GUIArrows>();

        if (guiArrows != null)
        {
            Debug.Log("found gui arrows");
            guiArrows.Hide();
        }

        if (targetOne != null && targetTwo != null)
        {
            targetOneImage = targetOne.GetComponentInChildren<Image>();
            targetTwoImage = targetTwo.GetComponentInChildren<Image>();

            targetOneImage.CrossFadeAlpha(0.0f, 0.1f, true);
            targetTwoImage.CrossFadeAlpha(0.0f, 0.1f, true);

            targetOne.SetActive(false);
            targetTwo.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) == true && targetOneFadedOut && loadScene == null)
        {
            currentTarget = 1;

            loadScene = SceneManager.LoadSceneAsync(2);
            loadScene.allowSceneActivation = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) == true && targetTwoFadedOut && loadScene == null)
        {
            currentTarget = 2;
            loadScene = SceneManager.LoadSceneAsync(2);
            loadScene.allowSceneActivation = false;
        }

        if (loadScene != null)
        {
            if (loadScene.progress >= 0.9f && targetOneFadedOut && targetTwoFadedOut)
            {
                switch (currentTarget)
                {
                    case 1:
                        targetOne.SetActive(true);
                        targetOneImage.CrossFadeAlpha(1.0f, 1.0f, true);
                        targetOneFadedOut = false;
                        guiArrows.SetDesiredDirection(targetTwo.transform); //set initial direction to target two to get user to look at target one
                        guiArrows.Show();
                        break;
                    case 2:
                        targetTwo.SetActive(true);
                        targetTwoImage.CrossFadeAlpha(1.0f, 1.0f, true);
                        targetTwoFadedOut = false;
                        guiArrows.SetDesiredDirection(targetOne.transform); //set initial direction to target one to get user to look at target one
                        guiArrows.Show();
                        break;
                    default:
                        Debug.Log("No target selected");
                        break;
                }
            }

            if (loadScene.progress >= 0.9f && Input.GetKeyDown(KeyCode.Alpha5))
            {
                loadScene.allowSceneActivation = true;
            }
            Debug.Log("progress: " + loadScene.progress);
        }
    }
}
