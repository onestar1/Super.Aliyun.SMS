using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using Super.Aliyun.SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Super.Aliyun.SMS.Services
{
    public class AliyunProviderOptionsConfigurations : IConfigureOptions<SmsProviderOptions>
    {
        private readonly ISiteService _siteService;

        public AliyunProviderOptionsConfigurations(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public void Configure(SmsProviderOptions options)
        {
            var typeOptions = new SmsProviderTypeOptions(typeof(AliyunSmsProvider));

            //var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
            //var settings = site.As<AliyunSettings>();
            var settings = _siteService.GetSettingsAsync<AliyunSettings>()
    .GetAwaiter()
    .GetResult();

            typeOptions.IsEnabled = settings.IsEnabled;

            options.TryAddProvider(AliyunSmsProvider.TechnicalName, typeOptions);
        }
    }
}
