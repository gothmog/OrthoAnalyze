using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;
using Orthologist.Bussiness.Services.Database;
using Orthologist.Bussiness.Services.Helpers;
using Orthologist.Bussiness.Services.Statistics;
using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Services.DataModifiers
{
    /// <summary>
    /// Služba, která má za úkol připravit všechna možná statistická data, od distančních matic až po intervaly statistických testů
    /// </summary>
    public class OrthoGroupModifierService : IOrthoGroupModifierService
    {
        IDatabaseProvider _databaseProvider;
        IRStatisticService _statisticService;
        SubstitutionMatrix _subMatrix = SubstitutionMatrixHelper.GetBlosum62Matrix();
        public OrthoGroupModifierService(IDatabaseProvider databaseProvider, IRStatisticService statisticService)
        {
            _databaseProvider = databaseProvider;
            _statisticService = statisticService;
        }

        public async Task WriteTreeToOrthoGroups()
        {
            const int maxDegreeOfParallelism = 1; // Počet paralelních úloh
            SemaphoreSlim semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            object lockObject = new object(); // Zámek pro thread-safety
            bool hasMoreData = true;

            var tasks = new List<Task>();

            //while (hasMoreData)
            //{
            //    OrthoGroupForAnalyze groupForAnalyze = null;

            //    lock (lockObject)
            //    {
            //        var groups = _databaseProvider.GetOrthoGroupForTreeUpdate(1);
            //        if (groups != null && groups.Any())
            //        {
            //            groupForAnalyze = groups.First();
            //        }
            //        else
            //        {
            //            hasMoreData = false;
            //        }
            //    }

            //    if (!hasMoreData)
            //    {
            //        break;
            //    }
            //    await semaphore.WaitAsync();

            //    var task = Task.Run(() =>
            //    {
            //        try
            //        {
            //            WriteTreeToOrthoGroup(groupForAnalyze!);
            //        }
            //        finally
            //        {
            //            semaphore.Release();
            //        }
            //    });

            //    tasks.Add(task);

            //    tasks = tasks.Where(t => !t.IsCompleted).ToList();
            //}

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Matoda vytváří pomocí muscle normalizovanou distanční matici a následně phylogenetický strom a toto uloží do nového objektu OrthoGroupAnalyze
        /// </summary>
        /// <param name="groupForAnalyze">Orthoskupina uložená v DB</param>
        public async Task<OrthoGroupAnalyze> WriteTreeToOrthoGroup(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze = null)
        {
            try
            {
                var matrix = new MuscleMatrixService(_subMatrix).GenerateDistanceMatrix(groupForAnalyze);
                var tree = PhyloTreeHelper.BuildNewickTree(matrix.Matrix, groupForAnalyze.GeneRecords.Select(x => x.organism_name).ToArray());
                if (groupAnalyze == null)
                {
                    groupAnalyze = new OrthoGroupAnalyze(groupForAnalyze);
                }
                groupAnalyze.PhyloTree = tree;
                groupAnalyze.DistanceMatrix = matrix.Matrix;
                groupAnalyze.ZScoreMatrix = matrix.ZscoreMatrix;
                groupAnalyze.AllignedSequences = matrix.AllignedFasta;
                groupAnalyze.Created = DateTime.Now;
                await _databaseProvider.CreateOrthoObjectAsync<OrthoGroupAnalyze>(groupAnalyze);
                return groupAnalyze;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> CreateMatrixesForCanidateGeneMantel(OrthoGroupForAnalyze groupForAnalyze, GeneRecord candidate, string orgToLeft)
        {
            var matrix = new MuscleMatrixService(_subMatrix).GenerateDistanceMatrix(groupForAnalyze, "base");
            groupForAnalyze.GeneRecords = groupForAnalyze.GeneRecords.Where(x=> x.organism_name != orgToLeft).ToList();
            groupForAnalyze.GeneRecords.Add(candidate);
            var derivedMatrix = new MuscleMatrixService(_subMatrix).GenerateDistanceMatrix(groupForAnalyze, "derived");
            OrthoGroupMatrixComparation orthoGroupMatrixComparation = new OrthoGroupMatrixComparation()
            {
                BaseDistanceMatrix = matrix.Matrix,
                DerivedDistanceMatrix = derivedMatrix.Matrix,
                OrganismForLeft = orgToLeft,
                OrganismForReplace = candidate.organism_name
            };
            await _databaseProvider.CreateOrthoObjectAsync<OrthoGroupMatrixComparation>(orthoGroupMatrixComparation);
            return true;
        }

        public async Task<bool> CreateZscoreMatrixesForCanidateGeneMantel(OrthoGroupForAnalyze groupForAnalyze, GeneRecord candidate, string orgToLeft)
        {
            var matrix = new MuscleMatrixService(_subMatrix).GenerateDistanceMatrix(groupForAnalyze, "base");
            groupForAnalyze.GeneRecords = groupForAnalyze.GeneRecords.Where(x => x.organism_name != orgToLeft).ToList();
            groupForAnalyze.GeneRecords.Add(candidate);
            var derivedMatrix = new MuscleMatrixService(_subMatrix).GenerateDistanceMatrix(groupForAnalyze, "derived");
            OrthoGroupMatrixComparation orthoGroupMatrixComparation = new OrthoGroupMatrixComparation()
            {
                BaseDistanceMatrix = matrix.ZscoreMatrix,
                DerivedDistanceMatrix = derivedMatrix.ZscoreMatrix,
                OrganismForLeft = orgToLeft,
                OrganismForReplace = candidate.organism_name
            };
            await _databaseProvider.CreateOrthoObjectAsync<OrthoGroupMatrixComparation>(orthoGroupMatrixComparation);
            return true;
        }

        #region StaticticPreparation

        public async Task<bool> CreatePermanovaTresholdForOrthoGroup(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze)
        {
            if (groupAnalyze.PermanovaOrthoGroupResult != null) return true;
            if (groupAnalyze.DistanceMatrix == null)
            {
                await WriteTreeToOrthoGroup(groupForAnalyze, groupAnalyze);
            }
            var originalMatrix = groupAnalyze.DistanceMatrix;
            (int leftOutlier, int rightOutlier) = GetOutlierFromDistanceMatrix(originalMatrix);

            //Příprava sady distančních matic pro permanovu (nahrazení left a potom right outlieru v řádcích a sloupcích distanční matice bez za prvé levého outlieru a za druhé bez pravého
            IList<MatrixRequest> permanovaMatrixesWithOutlier = new List<MatrixRequest>();
            for (int i = 0; i < originalMatrix.GetLength(0); i++) 
            {
                if (i != leftOutlier)
                {
                    MatrixRequest matrix = new MatrixRequest();
                    matrix.DerivedMatrix = ReplaceDistanceMatrix(originalMatrix, i, leftOutlier);
                    permanovaMatrixesWithOutlier.Add(matrix);
                }
            }
            var permanovaLeft = await _statisticService.ProcessPermanova(new Classes.Statistics.PermanovaRequest() { Matrixes = permanovaMatrixesWithOutlier });
            permanovaMatrixesWithOutlier = new List<MatrixRequest>();
            for (int i = 0; i < originalMatrix.GetLength(0); i++)
            {
                if (i != rightOutlier)
                {
                    MatrixRequest matrix = new MatrixRequest();
                    matrix.DerivedMatrix = ReplaceDistanceMatrix(originalMatrix, i, rightOutlier);
                    permanovaMatrixesWithOutlier.Add(matrix);
                }
            }
            var permanovaRight = await _statisticService.ProcessPermanova(new Classes.Statistics.PermanovaRequest() { Matrixes = permanovaMatrixesWithOutlier });
            groupAnalyze.PermanovaOrthoGroupResult = new Classes.Statistics.PermanovaOrthoGroupResult()
            {
                FStatLeft = Math.Min(permanovaLeft.Fstat, permanovaRight.Fstat),
                FStatRight = Math.Max(permanovaLeft.Fstat, permanovaRight.Fstat),
                PValueLeft = permanovaLeft.PValue,
                PValueRight = permanovaRight.PValue
            };
            return true;
        }

        public async Task<bool> CreateMantelTresholdForOrthoGroup(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze, bool withZscore = false)
        {
            if (groupAnalyze.MantelOrthoGroupResult != null) return true;
            if (groupAnalyze.DistanceMatrix == null)
            {
                await WriteTreeToOrthoGroup(groupForAnalyze, groupAnalyze);
            }
            var originalMatrix = !withZscore ? groupAnalyze.DistanceMatrix : groupAnalyze.ZScoreMatrix;
            (int leftOutlier, int rightOutlier) = GetOutlierFromDistanceMatrix(originalMatrix);

            // Matice bez outlierů
            double[,] matrixLeft = RemoveRowAndColumn(originalMatrix, leftOutlier);
            double[,] matrixRight = RemoveRowAndColumn(originalMatrix, rightOutlier);

            // Najdeme "průměrný" řádek
            int avgRowIndexLeft = FindAverageRow(matrixLeft);
            int avgRowIndexRight = FindAverageRow(matrixRight);

            // Duplikujeme průměrný řádek/sloupec, abychom zachovali dimenzi
            double[,] extendedLeft = DuplicateRowAndColumn(matrixLeft, avgRowIndexLeft);
            double[,] extendedRight = DuplicateRowAndColumn(matrixRight, avgRowIndexRight);
            string orig = OrthoGroupAnalyze.ConvertMatrixToJsonString(originalMatrix);
            string left = OrthoGroupAnalyze.ConvertMatrixToJsonString(extendedLeft);
            string right = OrthoGroupAnalyze.ConvertMatrixToJsonString(extendedRight);
            // Mantelovy testy
            var mantelLeft = await _statisticService.ProcessMantel(new Classes.Statistics.MantelRequest() { BaseMatrix = originalMatrix, DerivedMatrix = extendedLeft });
            var mantelRight = await _statisticService.ProcessMantel(new Classes.Statistics.MantelRequest() { BaseMatrix = originalMatrix, DerivedMatrix = extendedRight });

            groupAnalyze.MantelOrthoGroupResult = new Classes.Statistics.MantelOrthoGroupResult()
            {
                PearsonLeftCoeficient = Math.Min(mantelLeft.CorelationCoeficient, mantelRight.CorelationCoeficient),
                PearsonRightCoeficient = Math.Max(mantelLeft.CorelationCoeficient, mantelRight.CorelationCoeficient),
                PValueLeft = mantelLeft.PValue,
                PValueRight = mantelRight.PValue
            };
            return true;
        }

        int FindAverageRow(double[,] matrix)
        {
            int size = matrix.GetLength(0);
            double[] avgDistances = new double[size];

            for (int i = 0; i < size; i++)
            {
                double sum = 0;
                for (int j = 0; j < size; j++)
                    if (i != j)
                        sum += matrix[i, j];

                avgDistances[i] = sum / (size - 1);
            }

            double globalAvg = avgDistances.Average();
            return Array.IndexOf(avgDistances, avgDistances.OrderBy(x => Math.Abs(x - globalAvg)).First());
        }

        double[,] DuplicateRowAndColumn(double[,] matrix, int rowIndex)
        {
            int size = matrix.GetLength(0);
            double[,] newMatrix = new double[size + 1, size + 1];

            for (int i = 0, ni = 0; i < size; i++, ni++)
            {
                for (int j = 0, nj = 0; j < size; j++, nj++)
                {
                    newMatrix[ni, nj] = matrix[i, j];
                }
            }

            // Duplikujeme vybraný řádek a sloupec
            for (int j = 0; j < size; j++)
            {
                newMatrix[size, j] = matrix[rowIndex, j]; // Nový řádek
                newMatrix[j, size] = matrix[j, rowIndex]; // Nový sloupec
            }
            newMatrix[size, size] = matrix[rowIndex, rowIndex]; // Diagonální hodnota

            return newMatrix;
        }

        double[,] RemoveRowAndColumn(double[,] matrix, int index)
        {
            int size = matrix.GetLength(0);
            double[,] newMatrix = new double[size - 1, size - 1];

            for (int i = 0, ni = 0; i < size; i++)
            {
                if (i == index) continue;
                for (int j = 0, nj = 0; j < size; j++)
                {
                    if (j == index) continue;
                    newMatrix[ni, nj++] = matrix[i, j];
                }
                ni++;
            }
            return newMatrix;
        }

        private double[,] ReplaceDistanceMatrix(double[,] matrix, int replaceIndex, int useIndex)
        {
            int size = matrix.GetLength(0);
            if (size <= 1)
                throw new ArgumentException("Matice je příliš malá pro redukci.");
            if (replaceIndex < 0 || replaceIndex >= size || useIndex < 0 || useIndex >= size)
                throw new ArgumentOutOfRangeException("Indexy jsou mimo rozsah matice.");

            int newSize = size - 1;
            double[,] newMatrix = new double[newSize, newSize];

            for (int i = 0, ni = 0; i < size; i++)
            {
                if (i == replaceIndex) continue; // Přeskočíme nahrazovaný řádek

                for (int j = 0, nj = 0; j < size; j++)
                {
                    if (j == replaceIndex) continue; // Přeskočíme nahrazovaný sloupec

                    // Pokud kopírujeme řádek nahrazovaného indexu, použijeme hodnoty z useIndex
                    newMatrix[ni, nj] = (i == useIndex) ? matrix[useIndex, j] : (j == useIndex ? matrix[i, useIndex] : matrix[i, j]);
                    nj++;
                }
                ni++;
            }
            return newMatrix;
        }

        private (int, int) GetOutlierFromDistanceMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            // Pole pro uložení průměrných vzdáleností každého řádku
            double[] averageDistances = new double[rows];

            // Výpočet průměrné vzdálenosti pro každý řádek
            for (int i = 0; i < rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < cols; j++)
                {
                    if (i != j) // Nezahrnujeme vlastní vzdálenost (0.0 na diagonále)
                        sum += matrix[i, j];
                }
                averageDistances[i] = sum / (cols - 1);
            }

            // Najdeme indexy nejmenší a největší průměrné vzdálenosti
            int leftmost = 0, rightmost = 0;
            double minDist = averageDistances[0], maxDist = averageDistances[0];

            for (int i = 1; i < rows; i++)
            {
                if (averageDistances[i] < minDist)
                {
                    minDist = averageDistances[i];
                    leftmost = i;
                }
                if (averageDistances[i] > maxDist)
                {
                    maxDist = averageDistances[i];
                    rightmost = i;
                }
            }

            return (leftmost, rightmost);
        }

        //TODO? Fisher test - Permanova + Mantel
        //TODO? PCA - analýza variability


        #endregion
    }
}
