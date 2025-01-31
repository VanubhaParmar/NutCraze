#if UNITY_IOS
using Unity.Notifications.iOS;
#else
using Unity.Notifications.Android;
#endif
using UnityEngine.Android;
using UnityEngine;
using Sirenix.OdinInspector;

namespace com.tag.nut_sort {
    public class LocalNotificationManager : Manager<LocalNotificationManager>
    {
        #region PUBLIC_VARS

        public bool isSetNotification = true;
        public BaseNotification[] _baseNotifications;

        public bool IsNotificationOn
        {
            get
            {
                //return SettingsDataManager.Instance.IsNotificationOn;
                return true;
            }
        }

        #endregion

        #region PRIVATE_VARS

        private bool isInit = false;
        private bool isFromPause = false;

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            Init();
            OnLoadingDone();
        }

        //DO not delete this code
#if !UNITY_EDITOR
        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && isSetNotification && isFromPause)
            {
                CancelAllNotification();
                isFromPause = false;
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isSetNotification)
            {
                isFromPause = true;
                if(isInit)
                {
                     SendAllNotifications();
                }
            }
        }
#endif

        #endregion

        #region PUBLIC_FUNCTIONS

        public void Init()
        {
            CreateAndRegisterNotificationChannel(LocalNotificationConstans.default_ChannelId, LocalNotificationConstans.default_Name, LocalNotificationConstans.default_Description);
            CancelAllNotification();
            isInit = true;
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }

        public void SendNotification(BaseNotificationSO baseNotificationSO, int notificationTimeInSeconds)
        {
            SendNotification(baseNotificationSO.title, baseNotificationSO.GetNotificationText(), notificationTimeInSeconds);
        }

        public void SendNotification(BaseNotificationSO baseNotificationSO, string text, int notificationTimeInSeconds)
        {
            SendNotification(baseNotificationSO.title, text, notificationTimeInSeconds);
        }

        [Button]
        public void SendAllNotifications()
        {
            //Debug.Log("SendAllNotifications Before Cancel");
            CancelAllNotification();
            //Debug.Log("SendAllNotifications After Cancel");
            if (IsNotificationOn)
            {
                for (int i = 0; i < _baseNotifications.Length; i++)
                    _baseNotifications[i].GenerateNotification();
            }
        }

        #endregion

        #region NOTIFICATION   

        private void CreateAndRegisterNotificationChannel(string channelId, string name, string description)
        {
#if UNITY_IOS
       iOSNotificationTimeIntervalTrigger timeTriggers = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, 10),
            Repeats = false,
        };
#else
            // Create Notification Channel   
            AndroidNotificationChannel androidNotificationChannel = new AndroidNotificationChannel()
            {
                Id = channelId,
                Name = name,
                Importance = Importance.High,
                Description = description,
                EnableVibration = true,
            };
            // Register Notification Channel in Notification Center 
            AndroidNotificationCenter.RegisterNotificationChannel(androidNotificationChannel);
#endif
        }

        private int SendNotification(string titleText = LocalNotificationConstans.default_TitleText,
            string bodyText = LocalNotificationConstans.default_BodyText,
           long notificationTimeInSeconds = 5,
           string smallIcon = LocalNotificationConstans.default_smallIcon,
           string largeIcon = LocalNotificationConstans.default_largeIcon,
           string channelId = LocalNotificationConstans.default_ChannelId,
           bool isAutoCancelEnable = true)
        {
            if (notificationTimeInSeconds <= 0)
                return 0;
#if UNITY_IOS
        iOSNotificationTimeIntervalTrigger timeTriggers = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, (int)notificationTimeInSeconds),
            Repeats = false,
        };
        iOSNotification notification = new iOSNotification()
        {
            Identifier = channelId,
            Title = titleText,
            //Subtitle = bodyText,   
            Body = bodyText,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = channelId,
            ThreadIdentifier = channelId,
            Trigger = timeTriggers,
            Data = channelId,
        };
        iOSNotificationCenter.ScheduleNotification(notification);
        Debug.Log(notification.Data);
        return 1;
#else
            AndroidNotification notification = new AndroidNotification
            {
                Title = titleText,
                Text = bodyText
            };
            if (GetSDKInt() <= 28)
                notification.SmallIcon = smallIcon;
            else
                notification.SmallIcon = largeIcon;

            notification.LargeIcon = largeIcon;
            notification.FireTime = TimeManager.Now.AddSeconds(notificationTimeInSeconds);
            notification.ShouldAutoCancel = isAutoCancelEnable;
            notification.IntentData = channelId;
            int notificationId = AndroidNotificationCenter.SendNotification(notification, channelId);
            //Debug.Log(notification.IntentData + " getSDKInt : " + GetSDKInt());
            return notificationId;
#endif
        }

        private int GetSDKInt()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
#endif
            return 0;
        }

        private void CancelAllNotification()
        {
            Debug.Log("CancelAllNotifications");
            /* Cancel all notifications scheduled or previously shown by the app.
               All scheduled notifications will be canceled. 
               All notifications shown by the app will be removed from the status bar.*/
#if UNITY_IOS
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#else
            AndroidNotificationCenter.CancelAllNotifications();
#endif
        }

        private void GetNotificationData()
        {
#if UNITY_IOS
             var n = iOSNotificationCenter.GetLastRespondedNotification();
             if (n != null)
             {
                //var msg = "Last Received Notification : " + n.Identifier + "\n";
                //msg += "\n - Notification received: ";
                //msg += "\n - .Title: " + n.Title;
                //msg += "\n - .Body: " + n.Body;
                //msg += "\n - .CategoryIdentifier: " + n.CategoryIdentifier;
                //msg += "\n - .ThreadIdentifier: " + n.ThreadIdentifier;
                //msg += "\n - .Data: " + n.Data;
                //Debug.Log(msg);
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("notification_type", n.Data + "");
                //FlurryManager.Instance.LogEvent("Local_Notification", param);
                Debug.Log("========== 4 notification_type = " + n.Data);
             }
#else
            //var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
            //if (notificationIntentData != null)
            //{
            //    Debug.Log("========== notification_type = " + notificationIntentData.Notification.IntentData);
            //}
#endif
        }
        #endregion

        class LocalNotificationConstans
        {
            public const string default_ChannelId = "Notifications";
            public const string default_Name = "Notifications";
            public const string default_Description = "Notifications";
            public const string default_TitleText = "Nut Craze: Color Sorting Game";
            public const string default_BodyText = "Let's Play Game.";
            public const string default_smallIcon = "ic_push_small";
            public const string default_largeIcon = "ic_push_large";
        }
    }
}
//https://github.com/iammert/RadioPlayerService/wiki/Detects-when-app-is-killed-when-swiping