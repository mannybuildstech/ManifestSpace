using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
#if UNITY_EDITOR 
using UnityEditor;
#endif

public class AdBuddizBinding {

	public enum ABLogLevel { Info, Error, Silent };

	#if UNITY_IOS
	[DllImport ("__Internal")] private static extern void _setLogLevel(string level);
	[DllImport ("__Internal")] private static extern void _setPublisherKey(string publisherKey);
	[DllImport ("__Internal")] private static extern void _setTestModeActive();
	[DllImport ("__Internal")] private static extern void _cacheAds();
	[DllImport ("__Internal")] private static extern bool _isReadyToShowAd();
	[DllImport ("__Internal")] private static extern bool _isReadyToShowAdWithPlacement(string placementId);
	[DllImport ("__Internal")] private static extern void _showAd();
	[DllImport ("__Internal")] private static extern void _showAdWithPlacement(string placementId);
	[DllImport ("__Internal")] private static extern void _logNative(string text);
	#endif

	#if UNITY_ANDROID
	private static AndroidJavaObject adBuddizPlugin;
	#endif

	static AdBuddizBinding()
	{
		#if UNITY_ANDROID
		if (Application.platform == RuntimePlatform.Android) {
			adBuddizPlugin = new AndroidJavaObject ("com.purplebrain.adbuddiz.sdk.AdBuddizUnityBinding");
		}
		#endif

		#if UNITY_ANDROID || UNITY_IOS
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			GameObject o = GameObject.Find("AdBuddizManager");
			if (o == null) {
				new GameObject("AdBuddizManager").AddComponent<AdBuddizManager>();
			}
		}
		#endif
	}
	
	public static void SetLogLevel(ABLogLevel level)
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			adBuddizPlugin.Call("setLogLevel", level.ToString());
			#endif

			#if UNITY_IOS
			_setLogLevel(level.ToString());
			#endif
		}
	}
	
	public static void SetAndroidPublisherKey(string publisherKey)
	{
		if (Application.platform == RuntimePlatform.Android) {
			#if UNITY_ANDROID
			adBuddizPlugin.Call("setPublisherKey", publisherKey);
			#endif
		}
	}

	public static void SetIOSPublisherKey(string publisherKey)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_IOS
			_setPublisherKey(publisherKey);
			#endif
		}
	}

	public static void SetTestModeActive()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			adBuddizPlugin.Call("setTestModeActive");
			#endif
		
			#if UNITY_IOS
			_setTestModeActive();
			#endif
		}
	}

	public static void CacheAds()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			adBuddizPlugin.Call("cacheAds");
			#endif

			#if UNITY_IOS
			_cacheAds();
			#endif
		}
	}
	
	public static bool IsReadyToShowAd() 
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			return adBuddizPlugin.Call<bool>("isReadyToShowAd");
			#endif

			#if UNITY_IOS
			return _isReadyToShowAd();
			#endif
		}

		return false;
	}
	
	public static bool IsReadyToShowAd(string placementId) 
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			return adBuddizPlugin.Call<bool>("isReadyToShowAd", placementId);
			#endif
		
			#if UNITY_IOS
			return _isReadyToShowAdWithPlacement(placementId);
			#endif
		}

		return false;
	}

	public static void ShowAd()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			adBuddizPlugin.Call("showAd");
			#endif

			#if UNITY_IOS
			_showAd();
			#endif
		} else {
			#if UNITY_EDITOR 
			EditorUtility.DisplayDialog ("Error", "ShowAd() only works on real device!", "OK");
			#endif
		}
	}
	
	public static void ShowAd(string placementId)
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			adBuddizPlugin.Call("showAd", placementId);
			#endif

			#if UNITY_IOS
			_showAdWithPlacement(placementId);
			#endif
		}
	}
	
	public static void LogNative(string text)
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			#if UNITY_ANDROID
			adBuddizPlugin.Call("logNative", text);
			#endif
			
			#if UNITY_IOS
			_logNative(text);
			#endif
		}
	}
}