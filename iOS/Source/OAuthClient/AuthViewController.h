//
//  ViewController.h
//  OAuthClient


#import <UIKit/UIKit.h>

@interface AuthViewController : UIViewController
@property (weak, nonatomic) IBOutlet UIWebView *webView;
@property NSString *name;
@property NSString *email;

@end

