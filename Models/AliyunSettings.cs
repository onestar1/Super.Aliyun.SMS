using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Super.Aliyun.SMS.Models
{
    public class AliyunSettings
    {
        public bool IsEnabled { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string SignName { get; set; }
        public string TemplateCode { get; set; }

    }
}
