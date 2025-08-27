using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

namespace Interactive.PuzzelShape
{
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

    public class MailchimpSubscription : MonoBehaviour
    {
        public GameObject subscriptionPanel;
        public TMP_InputField emailInputField;
        private string apiKey = "7dbb2cb0d5d4956444b71d786c93dab5-us14"; 
        private string listId = "d08ad56b0b"; 

        void Start()
        {
            if (PlayerPrefs.GetInt("hasOpenedGameBefore", 0) == 1)
            {
                ShowSubscriptionPanel();
            }
            else
            {
                PlayerPrefs.SetInt("hasOpenedGameBefore", 1);
            }
        }

        public void Subscribe()
        {
            string email = emailInputField.text;
            if (IsValidEmail(email))
            {
                StartCoroutine(SubscribeToMailchimp(email));
            }
            else
            {
                Debug.Log("Invalid email address");
            }
        }

    private IEnumerator SubscribeToMailchimp(string email)
    {
        string url = "https://us14.api.mailchimp.com/3.0/lists/d08ad56b0b/members/";
        Debug.Log("URL: " + url);
        string jsonData = $"{{\"email_address\":\"{email}\", \"status\":\"subscribed\"}}";
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "apikey " + apiKey);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Abonare reusita");
        }
        else
        {
            Debug.Log("Eroare: " + www.error);
            Debug.Log("Response: " + www.downloadHandler.text);
        }
    }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ShowSubscriptionPanel()
        {
            subscriptionPanel.SetActive(true);
        }
    }


}