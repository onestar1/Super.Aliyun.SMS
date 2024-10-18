using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using OrchardCore.Sms.ViewModels;
using Super.Aliyun.SMS.Models;
using Super.Aliyun.SMS.Services;
using Super.Aliyun.SMS.ViewModels;

namespace Super.Aliyun.SMS.DisplayDriver
{
    public class AliyunSettingsDisplayDriver : SiteDisplayDriver<AliyunSettings>
    {
        private readonly IShellReleaseManager _shellReleaseManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPhoneFormatValidator _phoneFormatValidator;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly INotifier _notifier;

        internal readonly IHtmlLocalizer H;
        internal readonly IStringLocalizer S;

        protected override string SettingsGroupId
            => SmsSettings.GroupId;

        public AliyunSettingsDisplayDriver(
            IShellReleaseManager shellReleaseManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IPhoneFormatValidator phoneFormatValidator,
            IDataProtectionProvider dataProtectionProvider,
            INotifier notifier,
            IHtmlLocalizer<AliyunSettingsDisplayDriver> htmlLocalizer,
            IStringLocalizer<AliyunSettingsDisplayDriver> stringLocalizer)
        {
            _shellReleaseManager = shellReleaseManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _phoneFormatValidator = phoneFormatValidator;
            _dataProtectionProvider = dataProtectionProvider;
            _notifier = notifier;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(ISite site, AliyunSettings settings, BuildEditorContext c)
        {
            return Initialize<AliyunSettingsViewModel>("AliyunSettings_Edit", model => {
                model.IsEnabled = settings.IsEnabled;
                model.AccessKeyId = settings.AccessKeyId;
                model.AccessKeySecret = settings.AccessKeySecret;
                model.SignName = settings.SignName;
                model.TemplateCode = settings.TemplateCode;
            }).Location("Content:5#Aliyun")
            .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings))
            .OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, AliyunSettings settings, UpdateEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings)) {
                return null;
            }

            var model = new AliyunSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);
            var hasChanges = settings.IsEnabled != model.IsEnabled;
            var smsSettings = site.As<SmsSettings>();

            if (!model.IsEnabled) {
                if (hasChanges && smsSettings.DefaultProviderName == TwilioSmsProvider.TechnicalName) {
                    await _notifier.WarningAsync(H["You have successfully disabled the default SMS provider. The SMS service is now disable and will remain disabled until you designate a new default provider."]);

                    smsSettings.DefaultProviderName = null;

                    site.Put(smsSettings);
                }

                settings.IsEnabled = false;
            } else {
                settings.IsEnabled = true;

                if (string.IsNullOrWhiteSpace(model.AccessKeyId)) {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.AccessKeyId), S["accessKeyId requires a value."]);
                }

                if (string.IsNullOrWhiteSpace(model.AccessKeySecret)) {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.AccessKeySecret), S["AccessKeySecret requires a value."]);
                }

                if (settings.SignName == null && string.IsNullOrWhiteSpace(model.SignName)) {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.SignName), S["SignName required a value."]);
                }
               
                // Has change should be evaluated before updating the value.
                hasChanges |= settings.AccessKeyId != model.AccessKeyId;
                hasChanges |= settings.AccessKeySecret != model.AccessKeySecret;
                hasChanges |= settings.SignName != model.SignName;
                hasChanges |= settings.TemplateCode != model.TemplateCode;

                settings.AccessKeyId = model.AccessKeyId;
                settings.AccessKeySecret = model.AccessKeySecret;
                settings.SignName = model.SignName;
                settings.TemplateCode = model.TemplateCode;

            }

            if (context.Updater.ModelState.IsValid && settings.IsEnabled && string.IsNullOrEmpty(smsSettings.DefaultProviderName)) {
                // If we are enabling the only provider, set it as the default one.
                smsSettings.DefaultProviderName = AliyunSmsProvider.TechnicalName;
                site.Put(smsSettings);
                hasChanges = true;
            }

            if (hasChanges) {
                _shellReleaseManager.RequestRelease();
            }

            return Edit(site, settings, context);
        }
    }
}
