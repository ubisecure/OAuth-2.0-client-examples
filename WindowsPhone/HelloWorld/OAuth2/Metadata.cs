using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld.OAuth2
{
    [DataContract]
    public class Metadata
    {
        [DataMember(Name = "issuer", IsRequired = false)]
        public string Issuer { get; set; }
        [DataMember(Name = "authorization_endpoint", IsRequired = false)]
        public string AuthorizationEndpoint { get; set; }
        [DataMember(Name = "token_endpoint", IsRequired = false)]
        public string TokenEndpoint { get; set; }
        [DataMember(Name = "userinfo_endpoint", IsRequired = false)]
        public string UserInfoEndpoint { get; set; }
        [DataMember(Name = "tokeninfo_endpoint", IsRequired = false)]
        public string TokenInfoEndpoint { get; set; }
    }
}
