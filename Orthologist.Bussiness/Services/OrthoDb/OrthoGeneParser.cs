using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Orthologist.Bussiness.Classes;

/// <summary>
/// Statická třída, která má za úkol vytvářet kolekci GeneRecords z dat OrthoDb - z restAPI
/// </summary>
public static class OrthoGeneParser
{
    /// <summary>
    /// Metoda vytvoří kolekci GeneRecords
    /// </summary>
    /// <param name="fileContent">JSON obsah z restAPI</param>
    /// <returns></returns>
    public static List<GeneRecord> GetFastaRecords(string fileContent)
    {
        var geneRecords = new List<GeneRecord>();
        var lines = Regex.Split(fileContent, @"\r\n|\r|\n");

        var currentBlock = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (currentBlock.Count > 0)
                {
                    var record = ParseBlock(currentBlock);
                    if (record != null)
                    {
                        geneRecords.Add(record);
                    }
                    currentBlock.Clear();
                }
            }
            else
            {
                currentBlock.Add(line);
            }
        }

        // Zpracování posledního bloku
        if (currentBlock.Count > 0)
        {
            var record = ParseBlock(currentBlock);
            if (record != null)
            {
                geneRecords.Add(record);
            }
        }

        return geneRecords;
    }

    private static GeneRecord ParseBlock(List<string> block)
    {
        if (block.Count == 0) return null;

        // Extrakce JSON části z prvního řádku
        string firstLine = block[0];
        int jsonStartIndex = firstLine.IndexOf('{');
        if (jsonStartIndex < 0) return null;

        string jsonPart = firstLine.Substring(jsonStartIndex);

        // Deserializace JSON do GeneRecord
        var record = JsonConvert.DeserializeObject<GeneRecord>(jsonPart);
        if (record != null)
        {
            // Složení sekvence FASTA z dalších řádků
            string fastaSequence = string.Join("", block.Skip(1)); // Spojení řádků mimo první

            // Uložení sekvence do `Fast` parametru
            record.Fasta = fastaSequence;

        }
        return record;
    }
}

