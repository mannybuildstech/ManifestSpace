using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdBuddizManager : MonoBehaviour {

	void OnApplicationPause(bool pause) {
		if (!pause) { 
			AdBuddizBinding.CacheAds();
		}
	}

	public void Awake() {
		DontDestroyOnLoad(this);
	}
	
	// Fired when an ad has been cached
	public delegate void DidCacheAd();
	public static event DidCacheAd didCacheAd;	

	// Fired when an ad has been shown
	public delegate void DidShowAd();
	public static event DidShowAd didShowAd;	

	// Fired when an ad can't be shown
	public delegate void DidFailToShowAd(string adBuddizError);
	public static event DidFailToShowAd didFailToShowAd;
	
	// Fired when an ad has been clicked
	public delegate void DidClick();
	public static event DidClick didClick;	
	
	// Fired when an ad has been hidden
	public delegate void DidHideAd();
	public static event DidHideAd didHideAd;	
	
	public void OnDidFailToShowAd(string adBuddizError) {
		if (didFailToShowAd != null) {
			didFailToShowAd(adBuddizError);
		}
	}
	
	public void OnDidCacheAd() {
		if (didCacheAd != null) {
			didCacheAd();
		}	
	}
	
	public void OnDidShowAd() {
		if (didShowAd != null) {
			didShowAd();
		}	
	}
	
	public void OnDidClick() {
		if (didClick != null) {
			didClick();
		}	
	}
	
	public void OnDidHideAd() {
		if (didHideAd != null) {
			didHideAd();
		}	
	}
}