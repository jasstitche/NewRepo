using Logic.IHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Helpers
{
    public class EmailConfiguration : IEmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }

        public string PopServer { get; set; }
        public int PopPort { get; set; }
        public string PopUsername { get; set; }
        public string PopPassword { get; set; }
        public bool SendEmail { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string SiteName { get; set; }
        public bool IsGmail { get; set; }
        public string OutlookUsername { get; set; }
    }

}
