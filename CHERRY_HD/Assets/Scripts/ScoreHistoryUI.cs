using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class ScoreHistoryUI : MonoBehaviour
{
    [SerializeField] private GameObject instructionCard;
    [SerializeField] private GameObject scoreHistoryPanel;
    [SerializeField] private TMP_Text scoreHistoryText;

    [ContextMenu("Test Show Scores")]
    private void TestShowScores()
    {
        ShowScores();
    }

    public void ShowScores()
    {
        Debug.Log("View Scores clicked");

        instructionCard.SetActive(false);
        scoreHistoryPanel.SetActive(true);

        // Display CSV content as plain text.
        scoreHistoryText.richText = true;
        string path = Path.Combine(Application.persistentDataPath, "session_results.csv");

        try
        {
            if (!File.Exists(path))
            {
                scoreHistoryText.text = "No previous scores yet.";
                return;
            }

            string[] lines = File.ReadAllLines(path);
            var display = new StringBuilder();
            int count = 0;
            display.AppendLine("<b>Date & Time<pos=360>Game<pos=580>Duration<pos=750>Score</b>");
            display.AppendLine();

            // Read newest results first, skip header
            for (int i = lines.Length - 1; i >= 1 && count < 8; i--)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                List<string> fields = ParseCsvLine(lines[i]);

                if (fields.Count != 4)
                    continue;

                display.AppendLine($"{fields[0]}<pos=360>{fields[1]}<pos=580>{fields[2]}s<pos=750>{fields[3]}");

                count++;
            }

            if (count > 0)
            {
                scoreHistoryText.text = display.ToString();
            }
            else
            {
                scoreHistoryText.text = "No previous scores yet.";
            }
        }
        catch (IOException ex)
        {
            scoreHistoryText.text = "Could not read saved scores.";
            Debug.LogWarning($"Could not  read scores: {ex.Message}");
        }
    }

    public void Back()
    {
        scoreHistoryPanel.SetActive(false);
        instructionCard.SetActive(true);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char character = line[i];

            if (character == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (character == ',' && !insideQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(character);
            }
        }
        fields.Add(field.ToString());
        return fields;
    }
}
