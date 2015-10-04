#import "AdBuddizWrapper.h"
#import "AdBuddiz.h"

@interface UnityAdBuddizDelegate : NSObject <AdBuddizDelegate>
@end

static UnityAdBuddizDelegate *delegate = [[UnityAdBuddizDelegate alloc] init];

void _setLogLevel(const char* level) {
    
    NSString *lvl = [NSString stringWithUTF8String:level];
    
    if ([@"Info" isEqualToString:lvl]) {
        [AdBuddiz setLogLevel:ABLogLevelInfo];
    } else if ([@"Error" isEqualToString:lvl]) {
        [AdBuddiz setLogLevel:ABLogLevelError];
    } else if ([@"Silent" isEqualToString:lvl]) {
        [AdBuddiz setLogLevel:ABLogLevelSilent];
    }
}

void _setPublisherKey(const char* publisherKey) {
    [AdBuddiz setPublisherKey:[NSString stringWithUTF8String:publisherKey]];
}

void _setTestModeActive() {
    [AdBuddiz setTestModeActive];
}

void _cacheAds() {
    [AdBuddiz setDelegate:delegate];
    [AdBuddiz cacheAds];
}

bool _isReadyToShowAd() {
    return [AdBuddiz isReadyToShowAd];
}

bool _isReadyToShowAdWithPlacement(const char* placement) {
    return [AdBuddiz isReadyToShowAd:[NSString stringWithUTF8String:placement]];
}

void _showAd() {
    [AdBuddiz showAd];
}

void _showAdWithPlacement(const char* placement) {
    [AdBuddiz showAd:[NSString stringWithUTF8String:placement]];
}

void _logNative(const char* text) {
    NSLog(@"AdBuddizSDK: %@", [NSString stringWithUTF8String:text]);
}

@implementation UnityAdBuddizDelegate
- (void)didCacheAd
{
    UnitySendMessage("AdBuddizManager", "OnDidCacheAd", "");
}
- (void)didShowAd
{
    UnitySendMessage("AdBuddizManager", "OnDidShowAd", "");
}
- (void)didClick
{
    UnitySendMessage("AdBuddizManager", "OnDidClick", "");
}
- (void)didHideAd
{
    UnitySendMessage("AdBuddizManager", "OnDidHideAd", "");
}
- (void)didFailToShowAd:(AdBuddizError)error
{
    UnitySendMessage("AdBuddizManager", "OnDidFailToShowAd", [[AdBuddiz nameForError:error] UTF8String]);
}
@end