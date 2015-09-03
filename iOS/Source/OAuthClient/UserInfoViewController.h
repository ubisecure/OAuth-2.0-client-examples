//
//  UserInfoViewController.h
//  OAuthClient
//
//
//

#import <UIKit/UIKit.h>

@interface UserInfoViewController : UIViewController
@property (weak, nonatomic) IBOutlet UIButton *backButton;
@property (weak, nonatomic) IBOutlet UILabel *nameLabel;
@property (weak, nonatomic) IBOutlet UILabel *emailLabel;
@property NSString *name;
@property NSString *email;

@end
