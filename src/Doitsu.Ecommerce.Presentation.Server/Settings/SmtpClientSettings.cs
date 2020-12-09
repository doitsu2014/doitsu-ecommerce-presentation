using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doitsu.Ecommerce.Presentation.Server.Settings
{
    public class SmtpClientSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }

        public bool EnableSsl { get; set; }
    }
}
