using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WeatherToolsPortable;

internal static class WeatherToolsSmokeTests
{
    private static int assertions;

    private static void Assert(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Near(double actual, double expected, double tolerance, string message)
    {
        Assert(Math.Abs(actual - expected) <= tolerance,
            message + " Expected " + expected.ToString("R", CultureInfo.InvariantCulture) +
            ", got " + actual.ToString("R", CultureInfo.InvariantCulture) + ".");
    }

    private static string Atcf(string tech, int tau, string wind, string pressure, string latitude)
    {
        return "WP, 01, 2024080812, 03, " + tech + ", " + tau.ToString(CultureInfo.InvariantCulture) +
            ", " + latitude + ", 1420E, " + wind + ", " + pressure;
    }

    private static void TestThermalCalculations()
    {
        double t = 78.0, rh = 79.0;
        double simple = 0.5 * (t + 61.0 + ((t - 68.0) * 1.2) + (rh * 0.094));
        double screeningAverage = (simple + t) / 2.0;
        HeatIndexResult belowThreshold = ThermalCalculator.CalculateHeatIndex(t, rh);
        Assert(!belowThreshold.UsesRothfuszRegression, "Below-threshold Heat Index should use the NOAA simple equation.");
        Near(belowThreshold.TemperatureFahrenheit, simple, 1e-9, "Below-threshold Heat Index must return the simple equation, not the screening average.");
        Assert(screeningAverage < 80.0, "Test fixture should take the simple-formula branch.");

        HeatIndexResult rothfusz = ThermalCalculator.CalculateHeatIndex(90.0, 70.0);
        Assert(rothfusz.UsesRothfuszRegression, "Warm humid Heat Index should use Rothfusz regression.");
        Near(rothfusz.TemperatureFahrenheit, 105.92, 0.02, "NOAA regression example.");

        HeatIndexResult lowHumidity = ThermalCalculator.CalculateHeatIndex(100.0, 10.0);
        HeatIndexResult highHumidity = ThermalCalculator.CalculateHeatIndex(85.0, 90.0);
        Assert(lowHumidity.AdjustmentFahrenheit < 0.0, "Low-humidity adjustment should reduce Heat Index.");
        Assert(highHumidity.AdjustmentFahrenheit > 0.0, "High-humidity adjustment should increase Heat Index.");

        Near(ThermalCalculator.RelativeHumidityFromDewPoint(25.6, 21.7), 79.07, 0.03, "Magnus-Tetens humidity sample.");
        Near(ThermalCalculator.CalculateSteadmanApparentTemperature(25.6, 79.0, 1.0), 28.45, 0.02, "Steadman apparent temperature sample.");
    }

    private static void TestDvtsParser()
    {
        var warnings = new List<string>();
        List<DvtsRecord> records = DvtsParser.Parse(
            "WP 01 202408081200 DVTS 1350N 14200E 80.0 5050 S0000 PGTW", warnings);
        Assert(records.Count == 1, "DVTS example should parse.");
        DvtsRecord record = records[0];
        Near(record.Latitude, 13.5, 1e-9, "DVTS latitude.");
        Near(record.Longitude, 142.0, 1e-9, "DVTS longitude.");
        Near(record.TNumber, 5.0, 1e-9, "DVTS T number.");
        Near(record.CINumber, 5.0, 1e-9, "DVTS CI number.");
        Assert(record.TrendCode == "S" && record.TrendChange == 0.0 && record.TrendHours == 0,
            "DVTS S0000 should parse as steady with zero change and duration.");
    }

    private static void TestAtcfParserAndTrendSelection()
    {
        string text = String.Join(Environment.NewLine, new[] {
            Atcf("BEST", 0, "80", "", "135N"),
            Atcf("BEST", 0, "", "950", "135N"), // duplicate wind-radii row with partial intensity
            Atcf("BEST", 12, "120", "930", "140N"),
            Atcf("AVNO", 0, "90", "945", "135N")
        });
        var warnings = new List<string>();
        List<AtcfRecord> records = AtcfParser.Parse(text, warnings);
        List<AtcfIntensityPoint> points = AtcfIntensityPoint.FromAtcfBestTrackRecords(records);
        Assert(points.Count == 1, "Only BEST/TAU=0 rows should be plotted and same-time radii rows deduplicated.");
        Assert(points[0].HasVmax && points[0].HasMslp, "Duplicate ATCF rows should merge missing intensity values.");
        Near(points[0].VmaxKnots, 80.0, 1e-9, "ATCF VMAX.");
        Assert(points[0].MslpHpa == 950, "ATCF MSLP.");

        var conflictWarnings = new List<string>();
        List<AtcfRecord> conflicting = AtcfParser.Parse(String.Join(Environment.NewLine, new[] {
            Atcf("BEST", 0, "80", "950", "135N"),
            Atcf("BEST", 0, "81", "950", "135N")
        }), warnings);
        points = AtcfIntensityPoint.FromAtcfBestTrackRecords(conflicting, conflictWarnings);
        Assert(points.Count == 1 && points[0].VmaxKnots == 80.0,
            "Conflicting same-time BEST VMAX should keep the first equally complete row.");
        Assert(conflictWarnings.Count == 1 && conflictWarnings[0].Contains("80") && conflictWarnings[0].Contains("81"),
            "Conflicting same-time BEST VMAX should be surfaced as a warning.");

        List<AtcfRecord> invalid = AtcfParser.Parse(Atcf("BEST", 0, "45", "0", "999N"), warnings);
        Assert(invalid.Count == 1, "ATCF row remains inspectable even if individual fields are invalid.");
        Assert(!invalid[0].HasMslp, "ATCF MSLP 0 is a missing value.");
        Assert(!invalid[0].HasLatitude, "ATCF latitude outside 0–90 degrees is invalid.");
    }

    private static void TestSectorParserYearWindow()
    {
        var warnings = new List<string>();
        List<AtcfSectorRecord> records = AtcfSectorParser.Parse(
            "WP012026 STORMNAME 991231 1200 13.5N 142.0E WP 80 950", warnings);
        Assert(records.Count == 1 && records[0].HasAnalysisTime, "NRL sector row should parse.");
        Assert(records[0].AnalysisTimeUtc.Year == 1999, "Two-digit year 99 should map to 1999.");

        records = AtcfSectorParser.Parse("WP012026 STORMNAME 000101 0000 13.5N 142.0E WP 80 0", warnings);
        Assert(records[0].AnalysisTimeUtc.Year == 2000, "Two-digit year 00 should map to 2000.");
        Assert(!records[0].HasMslp, "Out-of-range sector pressure should be missing, not plotted.");
        Assert(warnings.Any(warning => warning.Contains("0")), "Sector MSLP zero should have a missing-value warning.");
    }

    private static void TestLanguageKeyParity(string repositoryRoot)
    {
        string languageDirectory = Path.Combine(repositoryRoot, "src", "portable", "WeatherToolsPortable", "languages");
        string[] files = { "zh-Hant.xml", "zh-Hans.xml", "en-US.xml" };
        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (XElement element in XDocument.Load(Path.Combine(languageDirectory, files[0])).Root.Elements("string"))
            expected.Add((string)element.Attribute("key"));
        Assert(expected.Count > 0, "Traditional Chinese language package should contain keys.");
        foreach (string file in files.Skip(1))
        {
            var actual = new HashSet<string>(XDocument.Load(Path.Combine(languageDirectory, file)).Root
                .Elements("string").Select(element => (string)element.Attribute("key")), StringComparer.OrdinalIgnoreCase);
            Assert(expected.SetEquals(actual), "All language packs must have the same key set: " + file);
        }
    }

    public static int Main(string[] args)
    {
        try
        {
            string root = args.Length == 0 ? Directory.GetCurrentDirectory() : args[0];
            TestThermalCalculations();
            TestDvtsParser();
            TestAtcfParserAndTrendSelection();
            TestSectorParserYearWindow();
            TestLanguageKeyParity(root);
            Console.WriteLine("PASS: " + assertions + " assertions");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL: " + exception);
            return 1;
        }
    }
}
