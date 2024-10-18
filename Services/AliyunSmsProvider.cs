using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using OrchardCore.Sms;
using OrchardCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Super.Aliyun.SMS.Models;
using Microsoft.AspNetCore.Routing.Template;
using PhoneNumbers;
using AlibabaCloud.SDK.Dysmsapi20170525.Models;
using AlibabaCloud.SDK.Dysmsapi20170525;
using Tea;

namespace Super.Aliyun.SMS.Services
{
    /// <summary>
    /// 阿里云短信
    /// </summary>
    public class AliyunSmsProvider : ISmsProvider
    {
        public const string TechnicalName = "Aliyun";

        public const string ProtectorName = "Aliyun";

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
        };

        private readonly ISiteService _siteService;

        private readonly IDataProtectionProvider _dataProtectionProvider;

        private readonly ILogger<AliyunSmsProvider> _logger;

        private readonly IHttpClientFactory _httpClientFactory;

        protected readonly IStringLocalizer S;

        private AliyunSettings _settings;

        public LocalizedString Name => S["Aliyun"];

        public AliyunSmsProvider(ISiteService siteService, IDataProtectionProvider dataProtectionProvider, ILogger<AliyunSmsProvider> logger, IHttpClientFactory httpClientFactory, IStringLocalizer<AliyunSmsProvider> stringLocalizer)
        {
            _siteService = siteService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            S = stringLocalizer;
        }

        public async Task<SmsResult> SendAsync(SmsMessage message)
        {
            string templateCode = "";
            string signName = "";
             
            ArgumentNullException.ThrowIfNull(message, "message");
            if (string.IsNullOrEmpty(message.To)) {
                throw new ArgumentException("A phone number is required in order to send a message.");
            }

            if (string.IsNullOrEmpty(message.Body)) {
                throw new ArgumentException("A message body is required in order to send a message.");
            }

            try {
                AliyunSettings aliyunSettings = await GetSettingsAsync();

                if (message is AliyunSmsMessage aliyunMessage) {
                    // 访问 AliyunSmsMessage 的额外字段
                    templateCode = aliyunMessage.TemplateCode;
                    signName = aliyunMessage.SignName;
                }else {
                    templateCode = aliyunSettings.TemplateCode;
                    signName = aliyunSettings.SignName;
                }
                var aliyunclient = CreateClient(aliyunSettings);
                //var response = await aliyunclient.SendSmsAsync(request);
               SendSmsRequest sendSmsRequest = new SendSmsRequest {
                    PhoneNumbers = message.To,
                    SignName = signName,
                    TemplateCode= templateCode,
                    TemplateParam= message.Body
               };
                 var response = await  aliyunclient.SendSmsAsync(sendSmsRequest);

                //try {
                //    // 复制代码运行请自行打印 API 的返回值
                //    aliyunclient.SendSmsWithOptions(sendSmsRequest, new AlibabaCloud.TeaUtil.Models.RuntimeOptions());
                //} catch (TeaException error) {
                //    // 此处仅做打印展示，请谨慎对待异常处理，在工程项目中切勿直接忽略异常。
                //    // 错误 message
                //    Console.WriteLine(error.Message);
                //    // 诊断地址
                //    Console.WriteLine(error.Data["Recommend"]);
                //    AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
                //} catch (Exception _error) {
                //    TeaException error = new TeaException(new Dictionary<string, object>
                //    {
                //    { "message", _error.Message }
                //});
                //    // 此处仅做打印展示，请谨慎对待异常处理，在工程项目中切勿直接忽略异常。
                //    // 错误 message
                //    Console.WriteLine(error.Message);
                //    // 诊断地址
                //    Console.WriteLine(error.Data["Recommend"]);
                //    AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
                //}
                if (response.StatusCode == 200) {
                    return SmsResult.Success;
                } else {
                    _logger.LogError("Aliyun SMS service was unable to send SMS messages. Error code: {errorCode}, message: {errorMessage}", response.StatusCode, response.Body);
                    return SmsResult.Failed(S["SMS message was not send."]);
                }


            } catch (Exception ex) {
                _logger.LogError(ex, "Twilio service was unable to send SMS messages.");
                return SmsResult.Failed(S["SMS message was not send. Error: {0}", new object[1] { ex.Message }]);
            }
          //  return SmsResult.Failed(S["SMS message was not send. Error: {0}", new object[1] { "未配置" }]);

        }
        /// <summary>
        /// 返回请求客户端 根据具体的实现
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private HttpClient GetHttpClient(AliyunSettings settings)
        {
            string s = settings.AccessKeyId + ":" + settings.AccessKeySecret;
            string parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(s));
            HttpClient httpClient = _httpClientFactory.CreateClient("Aliyun");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", parameter);
            return httpClient;
        }
        public static  Client CreateClient(AliyunSettings settings)
        {
            // 工程代码泄露可能会导致 AccessKey 泄露，并威胁账号下所有资源的安全性。以下代码示例仅供参考。
            // 建议使用更安全的 STS 方式，更多鉴权访问方式请参见：https://help.aliyun.com/document_detail/378671.html。
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config {
                // 必填，请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_ID。
                AccessKeyId = settings.AccessKeyId,
                // 必填，请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_SECRET。
                AccessKeySecret = settings.AccessKeySecret,
            };
            // Endpoint 请参考 https://api.aliyun.com/product/Dysmsapi
            config.Endpoint = "dysmsapi.aliyuncs.com";
            return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
        }
        /// <summary>
        /// 获取设置
        /// </summary>
        /// <returns></returns>
        private async Task<AliyunSettings> GetSettingsAsync()
        {
            if (_settings == null) {
                AliyunSettings aliyunSettings = await _siteService.GetSettingsAsync<AliyunSettings>();
                IDataProtector protector = _dataProtectionProvider.CreateProtector("Aliyun");
                _settings = new AliyunSettings {
                    AccessKeyId = aliyunSettings.AccessKeyId,
                    AccessKeySecret = aliyunSettings.AccessKeySecret,
                    SignName = aliyunSettings.SignName,
                    TemplateCode=   aliyunSettings.TemplateCode,
                };
            }

            return _settings;
        }
    }

}
