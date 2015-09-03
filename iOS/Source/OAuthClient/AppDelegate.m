//
//  AppDelegate.m
//  OAuthClient
//
//
//

#import "AppDelegate.h"
#import "NXOAuth2AccountStore.h"
#import "Config.h"

@interface AppDelegate ()

@end

@implementation AppDelegate


- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    // Override point for customization after application launch.
    return YES;
}

- (void)applicationWillResignActive:(UIApplication *)application {
    // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
    // Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
}

- (void)applicationDidEnterBackground:(UIApplication *)application {
    // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later.
    // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
}

- (void)applicationWillEnterForeground:(UIApplication *)application {
    // Called as part of the transition from the background to the inactive state; here you can undo many of the changes made on entering the background.
}

- (void)applicationDidBecomeActive:(UIApplication *)application {
    // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
}

- (void)applicationWillTerminate:(UIApplication *)application {
    // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
}

+ (void)initialize {
    NSDictionary *configDict = @{
                                 kNXOAuth2AccountStoreConfigurationClientID: CONFIG_CLIENT_ID,
                                 kNXOAuth2AccountStoreConfigurationSecret: CONFIG_CLIENT_SECRET,
                                 kNXOAuth2AccountStoreConfigurationScope: [NSSet setWithObjects:CONFIG_RESOURCE_ID, nil],
                                 kNXOAuth2AccountStoreConfigurationAuthorizeURL: [NSURL URLWithString:CONFIG_AUTH_ENDPOINT],
                                 kNXOAuth2AccountStoreConfigurationTokenURL: [NSURL URLWithString:CONFIG_TOKEN_ENDPOINT],
                                 kNXOAuth2AccountStoreConfigurationRedirectURL: [NSURL URLWithString:CONFIG_REDIRECT_URI],
                                 kNXOAuth2AccountStoreConfigurationTokenType: @"Bearer",
                                 kNXOAuth2AccountStoreConfigurationCustomHeaderFields: [NSDictionary dictionaryWithObject:@"application/x-www-form-urlencoded" forKey:@"Content-Type"]
                                 };
    
    [[NXOAuth2AccountStore sharedStore] setConfiguration:configDict forAccountType:CONFIG_ACCOUNT];
}

@end