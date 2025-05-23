using UnityEngine;
using GoogleMobileAds.Api;
using System;
using JetBrains.Annotations;

public enum AdType
{
    Clear,
    Help,
    Systematic
}

public class AdsManager : MonoBehaviour
{

    public static Action onPlayerWarchedClearAd;
    public static Action onPlayerWatchedDoubleMoneyAd;

    public static Action onPlayerWatchedSystematicAd;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    public static string _adUnitId = "ca-app-pub-5781212170655183/7436128153";
#elif UNITY_IPHONE
    public static string _adUnitId = "ca-app-pub-5781212170655183/2343583576";
#else
    private string _adUnitId = "unused";
#endif

    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            LoadRewardedAd();
        });
    }

    private static RewardedAd _rewardedAd;

    /// <summary>
    /// Loads the rewarded ad.
    /// </summary>
    public static void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();


        // send the request to load the ad.
        RewardedAd.Load(_adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;
                RegisterEventHandlers(ad);
                RegisterReloadHandler(ad);
            });
    }

    public static void ShowRewardedAd(AdType type)
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                switch (type)
                {
                    case AdType.Clear:
                        onPlayerWarchedClearAd?.Invoke();
                        break;
                    case AdType.Help:
                        onPlayerWatchedDoubleMoneyAd?.Invoke();
                        break;
                }
            });
        }
    }

    private static void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    private static void RegisterReloadHandler(RewardedAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }
}
