using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class SessionCsvLogger
{
    public static void RecordSession(string game, int durationSeconds, int finalScore)
    {
        string path = Path.Combine(Application.persistentDataPath, "session_results.csv");
        bool needsHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

        using (var writer = File.AppendText(path))
        {
            if (needsHeader)
                writer.WriteLine("timestamp_utc,game,selected_duration_seconds,final_score");

            string gameField = "\"" + game.Replace("\"", "\"\"") + "\"";
            writer.WriteLine(FormattableString.Invariant(
                $"{DateTime.UtcNow:O},{gameField},{durationSeconds},{finalScore}"));
        }

        Debug.Log($"Session saved to: {path}");
    }
}