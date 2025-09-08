//using System.Collections;
//using System.Collections.Generic;
//using Unity.Notifications.iOS;
//using UnityEngine;

//namespace Interactive.DRagDrop
//{
//using System.Collections;
//using System.Collections.Generic;
//using Unity.Notifications.iOS;
//using UnityEngine;

//    public class NotificationManager : MonoBehaviour
//    {
//        private static NotificationManager _instance = null;
//        public static NotificationManager Instance
//        {
//            get
//            {
//                if (_instance == null)
//                {
//                    Instantiate(Resources.Load("Managers") as GameObject);
//                }
//                return _instance;
//            }
//            private set
//            {
//                _instance = value;
//            }
//        }

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


//        private void Awake()
//        {
//            if (_instance != null && _instance != this)
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

//        // Start is called before the first frame update
//        IEnumerator Start()
//        {
//            yield return RequestNotificationPermission();
//            ClearDailyNotifications();
//        }

//        // Update is called once per frame
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
//            Debug.Log("Registering Trial Notification: " + notificationDate.ToString());

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
//            Debug.Log("Create Trial Notification: " + purchaseTime.ToString());

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
//            Debug.Log("Register Daily Notifications!");

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
//            Debug.Log("Clear Daily Notifications!");

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
//            Debug.Log("Create Daily Notifications!");

//            if (State == eState.Initiating)
//            {
//                dailyNotificationsQueued = true;
//                return;
//            }

//            RegisterDailyNotifications();
//        }

//        void RegisterOneDailyNotification(System.DateTime date, string notificationId)
//        {
//            Debug.Log("Register One Daily Notification: " + date.ToString() + " : " + notificationId);

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
//                Body = "We miss you!\nLearn new letters and words today.", //"Continue learning, your kid deserves it.",
//                //Subtitle = "This is a subtitle, something, something important...",
//                ShowInForeground = false,
//                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
//                CategoryIdentifier = "gameReminders",
//                ThreadIdentifier = "mainThread",
//                Trigger = trigger,
//            };

//            iOSNotificationCenter.ScheduleNotification(notification);
//        }
//    }


//}