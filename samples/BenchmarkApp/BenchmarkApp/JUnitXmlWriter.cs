

using System.Globalization;
using System.Xml.Linq;

namespace BenchmarkApp;

/// <summary>
/// Result of a single benchmark test execution.
/// </summary>
/// <param name="TestName">The benchmark test name.</param>
/// <param name="Passed">Whether the test passed.</param>
/// <param name="FailureReason">The failure reason, or <c>null</c> if passed.</param>
/// <param name="ElapsedSeconds">Elapsed wall-clock time in seconds.</param>
/// <param name="Metrics">Collected metrics from the benchmark run.</param>
public record BenchmarkTestResult(
    string TestName,
    bool Passed,
    string? FailureReason,
    double ElapsedSeconds,
    IReadOnlyDictionary<string, object> Metrics);

/// <summary>
/// Writes benchmark results as JUnit XML for CI reporting.
/// </summary>
public static class JUnitXmlWriter
{
    /// <summary>
    /// Writes the given benchmark results to a JUnit XML file.
    /// </summary>
    /// <param name="outputPath">The file path to write to.</param>
    /// <param name="results">The benchmark results to include.</param>
    public static void Write(string outputPath, IReadOnlyList<BenchmarkTestResult> results)
    {
        var totalTests = results.Count;
        var totalFailures = results.Count(r => !r.Passed);
        var totalTime = results.Sum(r => r.ElapsedSeconds);

        var testCases = new List<XElement>();
        foreach (var result in results)
        {
            var testCase = new XElement("testcase",
                new XAttribute("name", result.TestName),
                new XAttribute("classname", "BenchmarkApp"),
                new XAttribute("time", result.ElapsedSeconds.ToString("F3", CultureInfo.InvariantCulture)));

            if (result.Metrics.Count > 0)
            {
                var properties = new XElement("properties");
                foreach (var (key, value) in result.Metrics)
                {
                    properties.Add(new XElement("property",
                        new XAttribute("name", key),
                        new XAttribute("value", Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)));
                }

                testCase.Add(properties);
            }

            if (!result.Passed)
            {
                var message = result.FailureReason ?? "Test failed";
                testCase.Add(new XElement("failure",
                    new XAttribute("message", message),
                    message));
            }

            testCases.Add(testCase);
        }

        var testSuite = new XElement("testsuite",
            new XAttribute("name", "BenchmarkApp"),
            new XAttribute("tests", totalTests),
            new XAttribute("failures", totalFailures),
            new XAttribute("time", totalTime.ToString("F3", CultureInfo.InvariantCulture)),
            testCases);

        var testSuites = new XElement("testsuites",
            new XAttribute("name", "BenchmarkApp"),
            new XAttribute("tests", totalTests),
            new XAttribute("failures", totalFailures),
            new XAttribute("time", totalTime.ToString("F3", CultureInfo.InvariantCulture)),
            testSuite);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), testSuites);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        doc.Save(outputPath);
    }
}
