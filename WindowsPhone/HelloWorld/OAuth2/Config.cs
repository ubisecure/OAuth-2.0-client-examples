using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld.OAuth2
{
    public class Config
    {
        public string CLIENT_ID { get; set; }
        public string CLIENT_SECRET { get; set; }
        public string REDIRECT_URI { get; set; }
        public string SCOPE { get; set; }
        public string METADATA_URI { get; set; }
        public string AUTHORIZE_URI { get; set; }
        public string TOKEN_URI { get; set; }
        public string USERINFO_URI { get; set; }
        public string TOKENINFO_URI { get; set; }
        public string TOKENINFO_CLIENT_ID { get; set; }
        public string TOKENINFO_CLIENT_SECRET { get; set; }
    }
}
