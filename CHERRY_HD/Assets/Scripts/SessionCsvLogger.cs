using System;
using System.Globalization;
using System.IO;
using UnityEngine;

// saves completed session results to a CSV file
public static class SessionCsvLogger
{
    // records the local date and time, game, selected duration and final score
    public static void RecordSession(string game, int durationSeconds, int finalScore)
    {
        string path = Path.Combine(Application.persistentDataPath, "session_results.csv");
        bool needsHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

        // appends results without overwriting previous sessions
        using (var writer = File.AppendText(path))
        {
            if (needsHeader)
                writer.WriteLine("timestamp_local,game,selected_duration_seconds,final_score");

            string gameField = "\"" + game.Replace("\"", "\"\"") + "\"";
            writer.WriteLine(FormattableString.Invariant(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{gameField},{durationSeconds},{finalScore}"));
        }

        Debug.Log($"Session saved to: {path}");
    }
}