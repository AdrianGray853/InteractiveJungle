using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using System;
using System.Collections.Generic;
using UnityEngine;
    //using UnityEngine.Purchasing;
    //using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

    public class ProductManagerShape : MonoBehaviour//, IDetailedStoreListener
    {
        public static ProductManagerShape Instance { get; private set; }

        public enum eState
        {
            Initiating = 0,
            FailedToInitialize = 1,
            Initialized = 2
        }

        public eState State { get; private set; } = eState.Initiating;
        public string SubscriptionText { get; private set; } = "#ErrorPrice";
        public bool IsSubscribed { get; private set; }

        public Action<bool> OnStoreResults; // bool success
        public Action OnRestoreDone;

        //IStoreController m_StoreController;
        //IAppleExtensions m_AppleStore;

        // Your subscription ID. It should match the id of your subscription in your store.
        public string subscriptionProductId = "com.mycompany.mygame.subscription";
        public string previousSubscriptionProductId = "weekly_4.99"; // This is old price that still needs to be active, should be exposed if more subscription types will be available, but for now it's just hardcoded for fast update...

        float FailSafeCounter;  // Used to prevent locking from IAP mechanism failing to report bad initialization

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }

            FailSafeCounter = 5.0f; // 5 seconds
        }


        private void Update()
        {
            if (State == eState.Initiating && FailSafeCounter > 0)
            {
                FailSafeCounter -= Time.deltaTime;
                if (FailSafeCounter < 0f)
                {
                    Debug.LogError("Failsafe Triggered! IAP Initialization took longer than 5 sec!");
                    State = eState.FailedToInitialize;
                }
            }
        }

        void Start()
        {
    #if UNITY_EDITOR
            State = eState.Initialized;
            IsSubscribed = true;

            System.Globalization.CultureInfo culture = GetCultureInfoFromISOCurrencyCode("USD"); //subscriptionProduct.metadata.isoCurrencyCode);
            if (culture != null)
            {
                SubscriptionText = "then " + 9.99d.ToString("C", culture) + "/week"; //subscriptionProduct.metadata.localizedPrice.ToString("C", culture);
            }
    #else
            State = eState.Initiating;
            InitializePurchasing();
    #endif
        }

        void InitializePurchasing()
        {
            //var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            //Debug.Log("Initializing Purchasing!");
            //// Add our purchasable product and indicate its type.
            //builder.AddProduct(subscriptionProductId, ProductType.Subscription);
            //builder.AddProduct(previousSubscriptionProductId, ProductType.Subscription);

            //UnityPurchasing.Initialize(this, builder);
        }

        public void BuySubscription()
        {
            //if (m_StoreController != null)
            //    m_StoreController.InitiatePurchase(subscriptionProductId);
            //else
            //    Debug.Log("Not initialized!");
        }

        public void RestoreSubscription()
        {
            //if (m_AppleStore == null)
            //{
            //    Debug.Log("Cannot restore as AppleStore not initiated!");
            //    if (OnRestoreDone != null)
            //        OnRestoreDone();
            //    return;
            //}
            //m_AppleStore.RestoreTransactions((result, errorMessage) =>
            //{
            //    if (OnRestoreDone != null)
            //        OnRestoreDone();
            //    if (result)
            //        Debug.Log("Restored Transaction!");
            //    else
            //        Debug.Log($"Restoring Transaction Failed!\n{errorMessage}");
            //});
        }

        public void RefreshReceipt()
        {
            //m_AppleStore.RefreshAppReceipt(
            //successMsg =>
            //{
            //    Debug.Log("Success Refresh!\n" + successMsg);

            //    UnityEngine.Purchasing.Security.AppleReceipt appleReceipt = new UnityEngine.Purchasing.Security.AppleReceiptParser().Parse(Convert.FromBase64String(successMsg));
            //    Debug.Log(appleReceipt.expirationDate);
            //    Debug.Log(appleReceipt.receiptCreationDate);
            //    Debug.Log(appleReceipt.inAppPurchaseReceipts.Length);

            //    foreach (var receipt in appleReceipt.inAppPurchaseReceipts)
            //    {
            //        Debug.Log("---------------");
            //        Debug.Log("ProductID: " + receipt.productID);
            //        Debug.Log("Is Free Trial: " + receipt.isFreeTrial);
            //        Debug.Log("Product Type: " + receipt.productType);
            //        Debug.Log("Purchase Date: " + receipt.purchaseDate.ToLocalTime());
            //        Debug.Log("Cancellation Date: " + receipt.cancellationDate.ToLocalTime());
            //        Debug.Log("Sub Expiration Date: " + receipt.subscriptionExpirationDate.ToLocalTime());
            //    }

            //    /*
            //    var subscriptionManager = new SubscriptionManager(successMsg, subscriptionProductId, null);

            //    // The SubscriptionInfo contains all of the information about the subscription.
            //    // Find out more: https://docs.unity3d.com/Packages/com.unity.purchasing@3.1/manual/UnityIAPSubscriptionProducts.html
            //    var info = subscriptionManager.getSubscriptionInfo();

            //    Debug.Log("SubInfo: " + info.getProductId().ToString());
            //    Debug.Log("getExpireDate: " + info.getExpireDate().ToString());
            //    Debug.Log("isSubscribed: " + info.isSubscribed().ToString()); 
            //    */
            //},
            //errorMsg =>
            //{
            //    Debug.Log("Error Refresh\n" + errorMsg);
            //});
        }

        public static System.Globalization.CultureInfo GetCultureInfoFromISOCurrencyCode(string code)
        {
            foreach (System.Globalization.CultureInfo ci in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.SpecificCultures))
            {
                System.Globalization.RegionInfo ri = new System.Globalization.RegionInfo(ci.LCID);
                if (ri.ISOCurrencySymbol == code)
                    return ci;
            }
            return null;
        }

        public void OnInitialized()//IStoreController controller, IExtensionProvider extensions)
        {
            //Debug.Log("In-App Purchasing successfully initialized");
            //m_StoreController = controller;
            //m_AppleStore = extensions.GetExtension<IAppleExtensions>();
            //State = eState.Initialized;

            //Product subscriptionProduct = m_StoreController.products.WithID(subscriptionProductId);
            //if (subscriptionProduct != null)
            //{
            //    System.Globalization.CultureInfo culture = GetCultureInfoFromISOCurrencyCode(subscriptionProduct.metadata.isoCurrencyCode);
            //    if (culture != null)
            //    {

            //        SubscriptionText = "then " + subscriptionProduct.metadata.localizedPrice.ToString("C", culture) + "/week";
            //    }
            //    else
            //    {
            //        // Fallback to just using localizedPrice decimal
            //        SubscriptionText = "then " + subscriptionProduct.metadata.localizedPriceString + "/week";
            //    }
            //}

            //CheckSubscription();
            //UpdateUI();
        }

        public void OnInitializeFailed()//InitializationFailureReason error)
        {
            Debug.Log("Initialize Failed!");
            //OnInitializeFailed(error, null);
        }

        //public void OnInitializeFailed()// InitializationFailureReason error, string message)
        //{
        //    //State = eState.FailedToInitialize;
        //    //var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

        //    //if (message != null)
        //    //{
        //    //    errorMessage += $" More details: {message}";
        //    //}

        //    //Debug.Log(errorMessage);
        //}

        //    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        //    {
        //        // Retrieve the purchased product
        //        var product = args.purchasedProduct;

        //        // Maybe check if it's restored and inform the user?
        //        IsSubscribed = true;

        //        Debug.Log($"Purchase Complete - Product: {product.definition.id}");
        //        if (OnStoreResults != null)
        //            OnStoreResults(true);

        //        var subscriptionManager = new SubscriptionManager(product, null);
        //        var info = subscriptionManager.getSubscriptionInfo();
        //        var expireDate = info.getExpireDate().ToLocalTime();
        //        ProgressManagerShape.Instance.SetExpireDate(expireDate.ToBinary());

        //        Debug.Log("Is free trial?: " + info.isFreeTrial());
        //        Debug.Log("Purchase date: " + info.getPurchaseDate().ToLocalTime());
        //        Debug.Log("Free trial period: " + info.getFreeTrialPeriod());


        //        if (info.isFreeTrial() == Result.True)
        //        { // TODO: This might not be correct as ProcessPurchase might come from restore or other events, find a better way to know when trial ends!
        //            NotificationManagerShape.Instance.CreateTrialNotification(info.getPurchaseDate().ToLocalTime());
        //        }

        //        UpdateUI();

        //        // We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.

        //#if UNITY_IOS
        //        NotificationManagerShape.Instance.RequestPermission();
        //#endif
        //        return PurchaseProcessingResult.Complete;
        //    }

        //public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        //{
        //    Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
        //    if (OnStoreResults != null)
        //        OnStoreResults(false);
        //}

        //public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        //{
        //    Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureDescription.reason}");
        //    if (OnStoreResults != null)
        //        OnStoreResults(false);
        //}

        //bool IsSubscribedTo()//Product subscription)
        //{
        //    //// If the product doesn't have a receipt, then it wasn't purchased and the user is therefore not subscribed.
        //    //if (subscription.receipt == null)
        //    //{
        //    //    return false;
        //    //}

        //    //Dictionary<string, string> dict = m_AppleStore.GetIntroductoryPriceDictionary();
        //    //string intro_json = (dict == null || !dict.ContainsKey(subscription.definition.storeSpecificId)) ? null : dict[subscription.definition.storeSpecificId];
        //    ////The intro_json parameter is optional and is only used for the App Store to get introductory information.
        //    //var subscriptionManager = new SubscriptionManager(subscription, intro_json);

        //    //// The SubscriptionInfo contains all of the information about the subscription.
        //    //// Find out more: https://docs.unity3d.com/Packages/com.unity.purchasing@3.1/manual/UnityIAPSubscriptionProducts.html
        //    //var info = subscriptionManager.getSubscriptionInfo();

        //    //Debug.Log("SubInfo: " + info.getProductId().ToString());
        //    //Debug.Log("getExpireDate: " + info.getExpireDate().ToString());
        //    //Debug.Log("isSubscribed: " + info.isSubscribed().ToString());


        //    //return info.isSubscribed() == Result.True;
        //}

        void CheckSubscription()
        {
            //var subscriptionProduct = m_StoreController.products.WithID(subscriptionProductId);

            //try
            //{
            //    IsSubscribed = IsSubscribedTo(subscriptionProduct);
            //    Debug.LogError(IsSubscribed ? $"You are subscribed to {subscriptionProductId}" : $"You are not subscribed to {subscriptionProductId}");
            //}
            //catch (StoreSubscriptionInfoNotSupportedException)
            //{
            //    var receipt = (Dictionary<string, object>)MiniJson.JsonDecode(subscriptionProduct.receipt);
            //    var store = receipt["Store"];
            //    Debug.Log(
            //        "Couldn't retrieve subscription information because your current store is not supported.\n" +
            //        $"Your store: \"{store}\"\n\n" +
            //        "You must use the App Store to be able to retrieve subscription information.");
            //}

            //// Old product id check
            //if (!IsSubscribed)
            //{
            //    subscriptionProduct = m_StoreController.products.WithID(previousSubscriptionProductId);

            //    try
            //    {
            //        IsSubscribed = IsSubscribed || IsSubscribedTo(subscriptionProduct);
            //        Debug.LogError(IsSubscribed ? $"You are subscribed to {previousSubscriptionProductId}" : $"You are not subscribed to {previousSubscriptionProductId}");
            //    }
            //    catch (StoreSubscriptionInfoNotSupportedException)
            //    {
            //        var receipt = (Dictionary<string, object>)MiniJson.JsonDecode(subscriptionProduct.receipt);
            //        var store = receipt["Store"];
            //        Debug.Log(
            //            "Couldn't retrieve subscription information because your current store is not supported.\n" +
            //            $"Your store: \"{store}\"\n\n" +
            //            "You must use the App Store to be able to retrieve subscription information.");
            //    }
            //}
        }

    #if DEVELOPMENT_BUILD
        public void FakePurchase()
        {
             IsSubscribed = true;
        }
    #endif

        void UpdateUI()
        {

        }
    }


}