using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

[SetUpFixture]
public class ExtentReportManager
{
    public static ExtentReports extent;
    public static ExtentTest? test;


    [OneTimeSetUp]
    public void SetUp()
    {
        ExtentSparkReporter htmlReporter = new ExtentSparkReporter("extentReport.html");
        extent = new ExtentReports();
        extent.AttachReporter(htmlReporter);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        extent.Flush();
    }
}