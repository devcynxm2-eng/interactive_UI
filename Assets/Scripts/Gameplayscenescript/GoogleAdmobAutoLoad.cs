using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GoogleAdmobAutoLoad : MonoBehaviour
{
    public static GoogleAdmobAutoLoad Instance;
    private bool isAdShowing = false;
    private bool isRewardedLoading = false;
    private bool isMobileAdsInitialized = false; // ✅ track init

    public string bannerAdId = "ca-app-pub-3940256099942544/6300978111";
    public string interstitialAdId = "ca-app-pub-3940256099942544/1033173712";
    public string rewardedAdId = "ca-app-pub-3940256099942544/5224354917";

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private Queue<(Action onRewarded, Action onFailed)> adQueue = new Queue<(Action, Action)>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        // ✅ Only initialize MobileAds ONCE ever
        if (!isMobileAdsInitialized)
        {
            isMobileAdsInitialized = true;
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("AdMob Initialized");
                LoadBanner();
                LoadInterstitial();
                LoadRewarded();
            });
        }

    }

    void Start()
    {
        //// ✅ Only initialize MobileAds ONCE ever
        //if (!isMobileAdsInitialized)
        //{
        //    isMobileAdsInitialized = true;
        //    MobileAds.Initialize(initStatus =>
        //    {
        //        Debug.Log("AdMob Initialized");
        //        LoadBanner();
        //        LoadInterstitial();
        //        LoadRewarded();
        //    });
        //}
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[AdMob] Scene loaded: {scene.name}");

        // ✅ Clear stale callbacks from destroyed scene objects
        adQueue.Clear();

        // ✅ Reset showing flag only — NOT isRewardedLoading
        // isRewardedLoading stays as-is so we don't double-load
        isAdShowing = false;

        // ✅ Only reload if ad is gone AND not already loading
        if (!isRewardedLoading && (rewardedAd == null || !rewardedAd.CanShowAd()))
        {
            Debug.Log("[AdMob] Reloading rewarded ad after scene change...");
            LoadRewarded();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        bannerView?.Destroy();
        interstitialAd?.Destroy();
        rewardedAd?.Destroy();
    }

    // ─── BANNER ───
    private void LoadBanner()
    {
        bannerView = new BannerView(bannerAdId, AdSize.Banner, AdPosition.Bottom);
        bannerView.LoadAd(new AdRequest());
        bannerView.Hide();
    }
    public void ShowBanner() { bannerView?.Show(); }
    public void HideBanner() { bannerView?.Hide(); }

    // ─── INTERSTITIAL ───
    private void LoadInterstitial()
    {
        InterstitialAd.Load(interstitialAdId, new AdRequest(), (ad, error) =>
        {
            if (error == null && ad != null)
            {
                interstitialAd = ad;
                interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitial();
            }
            else Debug.LogWarning("Interstitial failed: " + error);
        });
    }
    public void ShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd()) interstitialAd.Show();
        else Debug.Log("Interstitial not ready");
    }

    // ─── REWARDED ───
    private void LoadRewarded()
    {
        if (isRewardedLoading)
        {
            Debug.Log("[AdMob] Already loading rewarded, skipping...");
            return;
        }
        isRewardedLoading = true;
        Debug.Log("🔄 Loading rewarded ad...");

        RewardedAd.Load(rewardedAdId, new AdRequest(), (ad, error) =>
        {
            isRewardedLoading = false;

            if (error == null && ad != null)
            {
                rewardedAd = ad;
                Debug.Log("✅ Rewarded loaded");

                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Rewarded closed");
                    isAdShowing = false;
                    rewardedAd = null;
                    LoadRewarded();
                };

                rewardedAd.OnAdFullScreenContentFailed += (AdError err) =>
                {
                    Debug.LogWarning("Ad show failed: " + err);
                    isAdShowing = false;
                    rewardedAd = null;
                    LoadRewarded();

                    if (adQueue.Count > 0)
                    {
                        var next = adQueue.Dequeue();
                        next.onFailed?.Invoke();
                    }
                };

                ProcessQueue();
            }
            else
            {
                Debug.LogWarning("❌ Rewarded failed to load: " + error);
                isRewardedLoading = false;
                Invoke(nameof(LoadRewarded), 5f);

                while (adQueue.Count > 0)
                {
                    var pending = adQueue.Dequeue();
                    pending.onFailed?.Invoke();
                }
            }
        });
    }

    // ─── PUBLIC ───
    public void ShowRewarded(Action onRewarded, Action onFailed = null)
    {
        // ✅ Reject duplicates
        if (adQueue.Count > 0 || isAdShowing)
        {
            Debug.Log("⚠️ Ad already queued or showing. Ignoring.");
            return;
        }

        adQueue.Enqueue((onRewarded, onFailed));
        Debug.Log($"📥 Ad queued. Queue size: {adQueue.Count}");
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        if (adQueue.Count == 0 || isAdShowing) return;

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            isAdShowing = true;
            var next = adQueue.Dequeue();
            Debug.Log("▶️ Showing rewarded ad...");
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"✅ Reward granted: {reward.Amount}");
                next.onRewarded?.Invoke();
            });
        }
        else
        {
            Debug.Log("⏳ Ad not ready. Will play once loaded.");
            LoadRewarded();
        }
    }

    public bool IsRewardedReady() => rewardedAd != null && rewardedAd.CanShowAd();
}