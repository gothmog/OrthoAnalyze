using Nest;
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Orthologist.Bussiness.Services.DataModifiers
{
    /// <summary>
    /// Služba, která používá program muscle pro multisequence alignment
    /// </summary>
    public class MuscleMatrixService : IDataMatrixService
    {
        SubstitutionMatrix _subMatrix;
        DistanceMatrix DistanceMatrix = new DistanceMatrix();

        /// <summary>
        /// Konstruktor přijímá používanou substituční matici
        /// </summary>
        /// <param name="subMatrix"></param>
        public MuscleMatrixService(SubstitutionMatrix subMatrix)
        {
            _subMatrix = subMatrix;
        }

        public MuscleMatrixService()
        {

        }

        /// <summary>
        /// Generuje distanční matici pro konkrétní Orthogroupu s případným prefixem (pro vytváření dílčích matic)
        /// </summary>
        /// <param name="orthoGroup"></param>
        /// <returns></returns>
        public DistanceMatrix GenerateDistanceMatrix(OrthoGroupForAnalyze orthoGroup, string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            foreach (GeneRecord geneRecord in orthoGroup.GeneRecords)
            {
                sb.AppendLine(">" + geneRecord.organism_name);
                sb.AppendLine(geneRecord.Fasta);
            }
            File.WriteAllText($"{orthoGroup.Name}{prefix}.fasta", sb.ToString());
            RunMuscleProcess(orthoGroup, prefix);
            return DistanceMatrix;
        }

        private void RunMuscleProcess(OrthoGroupForAnalyze group, string prefix = "")
        {
            bool isLoaded = false;
            Process process = new Process();
            process.StartInfo.FileName = "muscle.exe";
            process.StartInfo.Arguments = $" -align {group.Name}{prefix}.fasta -output {group.Name}{prefix}align.fasta";
            process.Start();
            while (!isLoaded)
            {
                if (!File.Exists($"{group.Name}{prefix}align.fasta"))
                {
                    Thread.Sleep(100);
                }
                else
                {
                    List<string> sequences = PrepareSequnecesFromMuscle($"{group.Name}{prefix}align.fasta");
                    var matrix = CalculateDistanceMatrix(sequences.ToArray());
                    //matrix = NormalizeDistanceMatrix(matrix);
                    var zScoreMatrix = ComputeZScoreMatrix(CalculateNotNormalizedDistanceMatrix(sequences.ToArray()));
                    DistanceMatrix = new DistanceMatrix() { AllignedFasta = sequences.ToArray(), Matrix = matrix, Name = group.Name, Records = group.GeneRecords.ToList(), ZscoreMatrix = zScoreMatrix };
                    isLoaded = true;
                    File.Delete($"{group.Name}{prefix}align.fasta");
                    File.Delete($"{group.Name}{prefix}.fasta");
                    process.Close();
                }
            }
        }

        private List<string> PrepareSequnecesFromMuscle(string filePath)
        {
            {
                List<string> sequences = new List<string>();
                string currentSequence = string.Empty;

                foreach (var line in File.ReadLines(filePath))
                {
                    if (line.StartsWith(">"))
                    {
                        if (!string.IsNullOrEmpty(currentSequence))
                        {
                            sequences.Add(currentSequence);
                            currentSequence = string.Empty;
                        }
                    }
                    else
                    {
                        currentSequence += line.Trim();
                    }
                }
                if (!string.IsNullOrEmpty(currentSequence))
                {
                    sequences.Add(currentSequence);
                }
                File.Delete(filePath);
                return sequences;
            }
        }

        public static double[,] NormalizeDistanceMatrix(double[,] hlpDistanceMatrix)
        {
            int n = hlpDistanceMatrix.GetLength(0);
            double maxScore = double.MinValue;
            double minScore = double.MaxValue;

            // Najdi maximální a minimální skóre
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    maxScore = Math.Max(maxScore, hlpDistanceMatrix[i, j]);
                    minScore = Math.Min(minScore, hlpDistanceMatrix[i, j]);
                }
            }

            // Pokud je minScore záporné, přidáme jeho absolutní hodnotu
            double offset = Math.Abs(minScore);
            double range = maxScore + offset;

            // Normalizace vzdáleností na interval [0, 1]
            double[,] distanceMatrix = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // Přidáme offset a normalizujeme
                    distanceMatrix[i, j] = (maxScore - hlpDistanceMatrix[i, j]) / range;
                }
            }

            return distanceMatrix;
        }

        private double[,] CalculateDistanceMatrix(string[] alignedSequences)
        {
            int n = alignedSequences.Length;
            double[,] distanceMatrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (i == j)
                    {
                        distanceMatrix[i, j] = 0;
                    }
                    else
                    {
                        int score = CalculatePairwiseScore(alignedSequences[i], alignedSequences[j]);
                        distanceMatrix[i, j] = 1.0 - (double)score / GetMaxScore(alignedSequences[i]);
                        distanceMatrix[j, i] = distanceMatrix[i, j];
                    }
                }
            }

            return distanceMatrix;
        }

        #region ZScore

        private double[,] CalculateNotNormalizedDistanceMatrix(string[] alignedSequences)
        {
            int n = alignedSequences.Length;
            double[,] distanceMatrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (i == j)
                    {
                        distanceMatrix[i, j] = 0;
                    }
                    else
                    {
                        int score = CalculatePairwiseScore(alignedSequences[i], alignedSequences[j]);
                        distanceMatrix[i, j] = score;
                        distanceMatrix[j, i] = distanceMatrix[i, j];
                    }
                }
            }

            return distanceMatrix;
        }

        double[,] ComputeZScoreMatrix(double[,] kimuraMatrix)
        {
            int n = kimuraMatrix.GetLength(0);
            double sum = 0, sumSquared = 0;
            int count = 0;

            // Výpočet průměru a směrodatné odchylky
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j) // Nezahrnujeme diagonální prvky
                    {
                        sum += kimuraMatrix[i, j];
                        sumSquared += kimuraMatrix[i, j] * kimuraMatrix[i, j];
                        count++;
                    }
                }
            }

            double mean = sum / count;
            double variance = (sumSquared / count) - (mean * mean);
            double stdDev = Math.Sqrt(variance);

            // Výpočet Z-skóre matice
            double[,] zScoreMatrix = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        zScoreMatrix[i, j] = (kimuraMatrix[i, j] - mean) / stdDev;
                    else
                        zScoreMatrix[i, j] = 0; // Diagonální prvky jsou 0
                }
            }

            return zScoreMatrix;
        }

        #endregion

        private int CalculatePairwiseScore(string seq1, string seq2)
        {
            if (seq1.Length != seq2.Length)
                throw new ArgumentException("Aligned sequences must have the same length.");

            int totalScore = 0;
            for (int i = 0; i < seq1.Length; i++)
            {
                char aa1 = seq1[i];
                char aa2 = seq2[i];

                if (aa1 == '-' && aa2 == '-') continue;
                totalScore += _subMatrix.Matrix.TryGetValue((aa1, aa2), out int score) ? score : -4;
            }
            return totalScore;
        }

        private int GetMaxScore(string seq)
        {
            int maxScore = 0;
            foreach (char aa in seq)
            {
                if (aa != '-') maxScore += _subMatrix.Matrix.TryGetValue((aa, aa), out int score) ? score : -4;
            }
            return maxScore;
        }
    }
}
