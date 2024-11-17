using Amazon.CloudWatchLogs;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.AwsCloudWatch;

namespace Fußball.de.Rest.Api;

public static class LoggerBuilder
{
    public static Logger BuildLogger(bool isDevelopment)
    {
        if(isDevelopment)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
        }
        var cloudWatchClient = new AmazonCloudWatchLogsClient();
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.AmazonCloudWatch(
                logGroup: "/aws/lambda/fussballDeRestApi",
                logStreamPrefix: DateTime.Now.ToString("yyyyMMddHH:mm:ssfff"),
                cloudWatchClient: cloudWatchClient
            )
            .CreateLogger();
    }
}