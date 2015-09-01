using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld.OAuth2
{
    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "access_token", IsRequired = false)]
        public string AccessToken { get; set; }
    }
}
