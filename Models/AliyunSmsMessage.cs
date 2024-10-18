using OrchardCore.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Super.Aliyun.SMS.Models
{
    public class AliyunSmsMessage:SmsMessage
    {
        public string SignName { get; set; }
        public string TemplateCode { get; set; }

    }
}
