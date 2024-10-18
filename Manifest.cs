using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Super.Aliyun.SMS",
    Author = "hengdefeng-zxs",
    Website = "https://hengdefeng99.com",
    Version = "0.0.1"

)]


[assembly: Feature(
    Name = "Super Aliyun SMS",
    Id = "Super.Aliyun.SMS",
    Description = "Enables the ability to send SMS messages through aliyun Communication Services (aliyun).",
    Dependencies =
    [
        "OrchardCore.Sms",
    ],
    Category = "SMS"
)]

