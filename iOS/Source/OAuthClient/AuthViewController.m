//
//  ViewController.m
//  OAuthClient
//
//  Copyright © 2015 Nixu. All rights reserved.
//

#import "AuthViewController.h"
#import "NXOAuth2AccountStore.h"
#import "NXOAuth2Account.h"
#import "NXOAuth2AccessToken.h"
#import "UserInfoViewController.h"
#import "Config.h"

@interface AuthViewController () <UIWebViewDelegate>

@end

@implementation AuthViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(accessTokenRequestSucceeded:)
                                            name:NXOAuth2AccountStoreAccountsDidChangeNotification
                                            object:[NXOAuth2AccountStore sharedStore]];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(accessTokenRequestFailed:)
                                                 name:NXOAuth2AccountStoreDidFailToRequestAccessNotification
                                               object:[NXOAuth2AccountStore sharedStore]];
    self.webView.delegate = self;
}

- (void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:animated];
    NSHTTPCookieStorage *storage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
    for (NSHTTPCookie *cookie in [storage cookies]) {
        [storage deleteCookie:cookie];
    }
    [[NSUserDefaults standardUserDefaults] synchronize];
    
    [[NXOAuth2AccountStore sharedStore] requestAccessToAccountWithType:CONFIG_ACCOUNT
                                   withPreparedAuthorizationURLHandler:^(NSURL *preparedURL){
                                       NSURLRequest *urlRequest = [NSURLRequest requestWithURL:preparedURL];
                                       [self.webView loadRequest:urlRequest];
    }];
}

- (void)accessTokenRequestSucceeded:(NSNotification*)aNotification {
    NXOAuth2Account* account = [aNotification.userInfo objectForKey:@"NXOAuth2AccountStoreNewAccountUserInfoKey"];
    [self getTokenInfo:account.accessToken.accessToken];
}

- (void)accessTokenRequestFailed:(NSNotification*)aNotification {
    NSError *error = [aNotification.userInfo objectForKey:NXOAuth2AccountStoreErrorKey];

    UIAlertView *alert = [[UIAlertView alloc]
                          initWithTitle:[error localizedDescription]
                          message:[error localizedRecoverySuggestion]
                          delegate:nil
                          cancelButtonTitle:NSLocalizedString(@"Dismiss", @"")
                          otherButtonTitles:nil];
    [alert show];
}

- (void)getTokenInfo:(NSString*)accessToken {
    NSURL *url = [NSURL URLWithString:CONFIG_TOKENINFO_ENDPOINT];
    NSMutableURLRequest *request = [[NSMutableURLRequest alloc] initWithURL:url];
    request.HTTPMethod = @"POST";
    [request setValue:@"application/x-www-form-urlencoded" forHTTPHeaderField:@"Content-Type"];
    NSString *authStr = [NSString stringWithFormat:@"%@:%@", CONFIG_RESOURCE_ID, CONFIG_RESOURCE_SECRET];
    NSData *authData = [authStr dataUsingEncoding:NSUTF8StringEncoding];
    NSString *base64Auth = [authData base64EncodedStringWithOptions:0];
    
    NSString *authValue = [NSString stringWithFormat:@"Basic %@", base64Auth];
    [request setValue:authValue forHTTPHeaderField:@"Authorization"];
    
    NSString *postData = [NSString stringWithFormat:@"token=%@", accessToken];
    [request setHTTPBody:[NSData dataWithBytes:[postData UTF8String] length:strlen([postData UTF8String])]];
    
    NSOperationQueue *queue = [[NSOperationQueue alloc] init];
    
    [NSURLConnection sendAsynchronousRequest:request queue:queue completionHandler:^(NSURLResponse *response, NSData *data, NSError *error)
     {
         if (error != nil) {
             UIAlertView *alert = [[UIAlertView alloc]
                                   initWithTitle:[error localizedDescription]
                                   message:[error localizedRecoverySuggestion]
                                   delegate:nil
                                   cancelButtonTitle:NSLocalizedString(@"Dismiss", @"")
                                   otherButtonTitles:nil];
             [alert show];
             return;
         }
         NSError *parseError = nil;
         NSDictionary *dictionary = [NSJSONSerialization JSONObjectWithData:data options:0 error:&parseError];
         NSString *usn = [dictionary valueForKey:@"usn"];
         self.name = [[[[usn componentsSeparatedByString:@"CN="] objectAtIndex:1] componentsSeparatedByString:@","] firstObject];
         
         self.email = [dictionary valueForKey:@"mail"];
         [self performSegueWithIdentifier:@"showUserInfo" sender:self];
         
     }];
}

- (BOOL)webView:(UIWebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request navigationType:(UIWebViewNavigationType)navigationType
{
    if ([request.URL.absoluteString rangeOfString:@"code=" options:NSCaseInsensitiveSearch].location != NSNotFound) {
        NSString *url = [NSString stringWithFormat:@"%@?%@", CONFIG_REDIRECT_URI, request.URL.query];
        [[NXOAuth2AccountStore sharedStore] handleRedirectURL:[NSURL URLWithString:url]];
        return NO;
    }
    return YES;
}


- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    UserInfoViewController *controller = (UserInfoViewController*)segue.destinationViewController;
    controller.email = self.email;
    controller.name = self.name;
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

@end
