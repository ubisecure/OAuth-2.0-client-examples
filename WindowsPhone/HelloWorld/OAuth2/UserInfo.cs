using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld.OAuth2
{
    [DataContract]
    public class UserInfo
    {
        [DataMember(Name = "sub", IsRequired = false)]
        public string UserName { get; set; }
        [DataMember(Name = "email", IsRequired = false)]
        public string Mail { get; set; }
        [DataMember(Name = "mobile", IsRequired = false)]
        public string Mobile { get; set; }
        [DataMember(Name = "role", IsRequired = false)]
        public string[] Role { get; set; }
    }
}
