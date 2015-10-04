extern "C" {
    void _setLogLevel(const char* level);
    void _setPublisherKey(const char* publisherKey);
    void _setTestModeActive();
    void _cacheAds();
    bool _isReadyToShowAd();
    bool _isReadyToShowAdWithPlacement(const char* placement);
    void _showAd();
    void _showAdWithPlacement(const char* placement);
    void _logNative(const char* text);
}
