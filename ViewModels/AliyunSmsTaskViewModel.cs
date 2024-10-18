using OrchardCore.Sms.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Super.Aliyun.SMS.ViewModels
{
    public class AliyunSmsTaskViewModel : SmsTaskViewModel
    {
        public string TemplateCode { get; set; }
        public string SignName { get; set; }
    }
}
