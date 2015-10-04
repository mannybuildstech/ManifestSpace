using UnityEngine;

public class AdBuddizExample : MonoBehaviour {
	
	void Start() // Init SDK on app start
	{ 
		AdBuddizBinding.SetLogLevel(AdBuddizBinding.ABLogLevel.Info);         // log level
		AdBuddizBinding.SetAndroidPublisherKey("TEST_PUBLISHER_KEY_ANDROID"); // replace with your Android app publisher key
		AdBuddizBinding.SetIOSPublisherKey("TEST_PUBLISHER_KEY_IOS");         // replace with your iOS app publisher key
		AdBuddizBinding.SetTestModeActive();                                  // to delete before submitting to store
		AdBuddizBinding.CacheAds();                                           // start caching ads
	}

	public void OnGUI()
	{
		if (GUI.Button (new Rect (40, 40, Screen.width - 80, 80), "Show Ad") == true) {
			AdBuddizBinding.ShowAd();
		}
	}
	
	void OnEnable()
	{
		// Listen to AdBuddiz events
		AdBuddizManager.didFailToShowAd += DidFailToShowAd;
		AdBuddizManager.didCacheAd += DidCacheAd;
		AdBuddizManager.didShowAd += DidShowAd;
		AdBuddizManager.didClick += DidClick;
		AdBuddizManager.didHideAd += DidHideAd;
	}
	
	void OnDisable()
	{
		// Remove all event handlers
		AdBuddizManager.didFailToShowAd -= DidFailToShowAd;
		AdBuddizManager.didCacheAd -= DidCacheAd;
		AdBuddizManager.didShowAd -= DidShowAd;
		AdBuddizManager.didClick -= DidClick;
		AdBuddizManager.didHideAd -= DidHideAd;
	}
	
	void DidFailToShowAd(string adBuddizError) {
		AdBuddizBinding.LogNative("DidFailToShowAd: " + adBuddizError);
		Debug.Log ("Unity: DidFailToShowAd: " + adBuddizError);
	}
	
	void DidCacheAd() {
		AdBuddizBinding.LogNative("DidCacheAd");
		Debug.Log ("Unity: DidCacheAd");
	}

	void DidShowAd() {
		AdBuddizBinding.LogNative("DidShowAd");
		Debug.Log ("Unity: DidShowAd");
	}
	
	void DidClick() {
		AdBuddizBinding.LogNative("DidClick");
		Debug.Log ("Unity: DidClick");
	}
	
	void DidHideAd() {
		AdBuddizBinding.LogNative("DidHideAd");
		Debug.Log ("Unity: DidHideAd");
	}
}
