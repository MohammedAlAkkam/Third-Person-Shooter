using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CahngeScene : MonoBehaviour
{
    private int sceneCounter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            LoadScene();
        }
    }
    public void LoadScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(currentSceneIndex % 2);
        if (currentSceneIndex % 2 == 1)
            StartCoroutine(GetRequest());

    }

    IEnumerator GetRequest(string uri = @"https://api.countapi.xyz/hit/ThirdPersonShooter/key")
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
}
