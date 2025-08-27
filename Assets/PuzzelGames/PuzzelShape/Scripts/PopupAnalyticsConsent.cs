using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Interactive.PuzzelShape
{
using System;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

    // ReSharper disable ArrangeThisQualifier
    // ReSharper disable ArrangeTypeMemberModifiers



    public class PopupAnalyticsConsent : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Button buttonOk;
        [SerializeField] Button buttonClose;
        [SerializeField] TextMeshProUGUI textLinks;
        [SerializeField] string urlPrivacyPolicy;
        [SerializeField] string urlTermsOfService;

        private TaskCompletionSource<bool> tcs;

        //..............................................................................UNITY

        void Awake()
        {
            buttonOk   .onClick.AddListener( () => tcs!.SetResult(true)  );
            buttonClose.onClick.AddListener( () => tcs!.SetResult(false) );
            // use only ShowAsync() to activate object, ^~~~ or task source is null
            // if left enabled in scene, auto-disable it on scene load
            if (Time.timeSinceLevelLoad == 0f)
                this.gameObject.SetActive(false);
        }

        void OnValidate()
        {
            Debug.Assert(!string.IsNullOrEmpty(urlPrivacyPolicy), "privacy policy url missing", this);
            Debug.Assert(!string.IsNullOrEmpty(urlTermsOfService), "terms of service url missing", this);
        }

        [ContextMenu("Test Popup")]
        async void Test() => Debug.Log($"Popup closed with result '{ await ShowAsync() }'");

        //............................................................................PUBLIC

        public async Task<bool> ShowAsync()  // we don't expect cancellation for this method
        {
            Debug.Assert(tcs == null);
            tcs = new TaskCompletionSource<bool>();

            this.gameObject.SetActive(true);
            bool wasAccepted = await tcs.Task;
            this.gameObject.SetActive(false);

            tcs = null;
            return wasAccepted;
        }

        //..........................................................................INTERFACE

        public void OnPointerClick(PointerEventData ev)
        {
            int i = TMP_TextUtilities.FindIntersectingLink(textLinks, ev.position, null);
            if (i < 0) return;

            string id = textLinks.textInfo.linkInfo[i].GetLinkID();
            string url = id.ToLower() switch {
                { } s when s.Contains("policy") => urlPrivacyPolicy,
                { } s when s.Contains("terms")  => urlTermsOfService,
                _ => throw new InvalidOperationException($"unexpected link id '{id}'")
            };

            Application.OpenURL(url);
        }
    }


}