//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
////using Unity.Notifications.iOS;

//namespace Interactive.PuzzelShape
//{
//    // ReSharper disable ArrangeThisQualifier
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//#if UNITY_IOS
//    using Unity.Notifications.iOS;


//    public class NotificationManagerShape : MonoBehaviour
//    {
//        public static NotificationManagerShape Instance  { get; private set; }

//        public enum eState
//        {
//            Initiating,
//            Initialized,
//            Failed
//        }

//        public eState State { get; private set; } = eState.Initiating;

//        class QueueTrialEndNotification {
//            public System.DateTime notificationDate;
//            public bool Queued = false;
//        }

//        QueueTrialEndNotification trialEndNotificationQueue = new QueueTrialEndNotification();
//        bool dailyNotificationsQueued = false;

//        [SerializeField] bool enableEditorLogging;

//        private void Awake()
//        {
//            if (Instance != null && Instance != this)
//            {
//                Destroy(gameObject);
//            }
//            else
//            {
//                DontDestroyOnLoad(this);
//                Instance = this;
//            }
//        }

//        const string freeTrialReminderNotificationId = "Notification2DaysBeforeTrialExpirationId";

//        public void RequestPermission()
//        {
//            StartCoroutine(RequestNotificationPermission());
//            ClearDailyNotifications();
//        }

//        void Update()
//        {
//            if (State == eState.Initialized)
//            {
//                if (trialEndNotificationQueue.Queued)
//                {
//                    trialEndNotificationQueue.Queued = false;
//                    RegisterTrialNotification(trialEndNotificationQueue.notificationDate);
//                }
//                if (dailyNotificationsQueued)
//                {
//                    RegisterDailyNotifications();
//                    dailyNotificationsQueued = false;
//                }
//            }
//        }

//        private void OnApplicationFocus(bool focus)
//        {
//            if (focus)
//                ClearDailyNotifications();
//            else
//                RegisterDailyNotifications();
//        }

//        public IEnumerator RequestNotificationPermission()
//        {
//            State = eState.Initiating;
//            using (var request = new AuthorizationRequest(AuthorizationOption.Badge | AuthorizationOption.Sound | AuthorizationOption.Alert, false))
//            {
//                while (!request.IsFinished)
//                    yield return null;

//                State = request.Granted ? eState.Initialized : eState.Failed;
//            }
//        }

//        void RegisterTrialNotification(System.DateTime notificationDate)
//        {
//            this.Log("Registering Trial Notification: " + notificationDate);

//            iOSNotificationCenter.RemoveScheduledNotification(freeTrialReminderNotificationId);

//            var trigger = new iOSNotificationCalendarTrigger()
//            {
//                Day = notificationDate.Day,
//                Hour = notificationDate.Hour,
//                Minute = notificationDate.Minute,
//                Second = notificationDate.Second,
//                Year = notificationDate.Year,
//                Month = notificationDate.Month,
//                Repeats = false
//            };

//            var notification = new iOSNotification()
//            {
//                // You can specify a custom identifier which can be used to manage the notification later.
//                // If you don't provide one, a unique string will be generated automatically.
//                Identifier = freeTrialReminderNotificationId,
//                Title = "Trial Reminder",
//                Body = "Your trial expires in 2 days!",
//                //Subtitle = "This is a subtitle, something, something important...",
//                ShowInForeground = true,
//                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
//                CategoryIdentifier = "trialReminders",
//                ThreadIdentifier = "mainThread",
//                Trigger = trigger,
//            };

//            iOSNotificationCenter.ScheduleNotification(notification);
//        }

//        public void CreateTrialNotification(System.DateTime purchaseTime)
//        {
//            this.Log("Create Trial Notification: " + purchaseTime);

//            System.DateTime notificationTime = purchaseTime;
//            notificationTime.AddDays(5);

//            if (notificationTime < System.DateTime.Now)
//                return;

//            if (State == eState.Initiating)
//            {
//                trialEndNotificationQueue.notificationDate = notificationTime;
//                trialEndNotificationQueue.Queued = true;
//                return;
//            }

//            RegisterTrialNotification(notificationTime);
//        }

//        void RegisterDailyNotifications()
//        {
//            this.Log("Register Daily Notifications!");

//            System.DateTime now = System.DateTime.Now;
//            RegisterOneDailyNotification(now.AddDays(1), "NotificationBackToGameReminderDay1Id");
//            RegisterOneDailyNotification(now.AddDays(7), "NotificationBackToGameReminderDay7Id");
//            RegisterOneDailyNotification(now.AddDays(14), "NotificationBackToGameReminderDay14Id");
//            RegisterOneDailyNotification(now.AddDays(30), "NotificationBackToGameReminderDay30Id");
//            RegisterOneDailyNotification(now.AddDays(45), "NotificationBackToGameReminderDay45Id");
//            RegisterOneDailyNotification(now.AddDays(60), "NotificationBackToGameReminderDay60Id");
//        }

//        public void ClearDailyNotifications()
//        {
//            this.Log("Clear Daily Notifications!");

//            iOSNotificationCenter.RemoveScheduledNotification("NotificationBackToGameReminderDay1Id");
//            iOSNotificationCenter.RemoveScheduledNotification("NotificationBackToGameReminderDay7Id");
//            iOSNotificationCenter.RemoveScheduledNotification("NotificationBackToGameReminderDay14Id");
//            iOSNotificationCenter.RemoveScheduledNotification("NotificationBackToGameReminderDay30Id");
//            iOSNotificationCenter.RemoveScheduledNotification("NotificationBackToGameReminderDay45Id");
//            iOSNotificationCenter.RemoveScheduledNotification("NotificationBackToGameReminderDay60Id");
//            dailyNotificationsQueued = false;
//        }

//        public void CreateDailyNotifications()
//        {
//            this.Log("Create Daily Notifications!");

//            if (State == eState.Initiating)
//            {
//                dailyNotificationsQueued = true;
//                return;
//            }

//            RegisterDailyNotifications();
//        }

//        void RegisterOneDailyNotification(System.DateTime date, string notificationId)
//        {
//            this.Log($"Register One Daily Notification: {date} : {notificationId}");

//            iOSNotificationCenter.RemoveScheduledNotification(notificationId);

//            var trigger = new iOSNotificationCalendarTrigger()
//            {
//                Day = date.Day,
//                Hour = date.Hour,
//                Minute = date.Minute,
//                Second = date.Second,
//                Year = date.Year,
//                Month = date.Month,
//                Repeats = false
//            };

//            var notification = new iOSNotification()
//            {
//                Identifier = notificationId,
//                Title = Application.productName, //"Trial Reminder",
//                Body = GetNotificationText(), //"We miss you!\nLearn new letters and words today.", //"Continue learning, your kid deserves it.",
//                //Subtitle = "This is a subtitle, something, something important...",
//                ShowInForeground = false,
//                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
//                CategoryIdentifier = "gameReminders",
//                ThreadIdentifier = "mainThread",
//                Trigger = trigger,
//            };

//            iOSNotificationCenter.ScheduleNotification(notification);
//        }

//        string GetNotificationText()
//        {
//            string[] notifications =
//            {
//                "🚀🧩 Blast Off into Learning Fun!  Shapes, Colors & Puzzles await! Join now!",
//                "🎮🧠 Play & Learn: Shapes, Colors and Puzzles galore! The ultimate brainy adventure!",
//                "🌟🎨 Get Creative with Colors! Touch & Fill to make the world come alive!",
//                "🌈🧩 Discover the Magic of Shapes & Colors! Play now for endless joy!",
//                "🖍🌏 Colorful World Awaits! Touch & Fill your way to creativity!",
//                "📚🤩 Explore & Learn: Shapes, Colors and Puzzles made extra fun! Play now!"
//            };

//            return notifications.GetRandomElement();
//        }

//        private void Log(object message)
//        {
//            // disable annoying logging in editor play mode
//            #if UNITY_EDITOR
//            if (!Application.isPlaying || !enableEditorLogging) return;
//            #endif
//            Debug.Log(message);
//        }
//    }

//    #else

//    // just a dummy class to make project build on non-iOS platform
//    public class NotificationManagerShape : MonoBehaviour
//    {
//        public static NotificationManagerShape Instance  { get; private set; }
//        private void Awake() { Instance = this; }
//        public void CreateTrialNotification(System.DateTime purchaseTime) {}

//    }
//    #endif


//}