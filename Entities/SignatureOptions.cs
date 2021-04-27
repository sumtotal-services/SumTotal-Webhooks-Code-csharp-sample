using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebhookListener.Entities
{
    public class SignatureOptions
    {
        public SignatureOptions()
        {
        }
        public string SecretKey { get; set; }
    }
}
