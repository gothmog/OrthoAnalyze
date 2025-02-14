using Orthologist.Bussiness.Classes.Statistics;
using RDotNet;

namespace Orthologist.Bussiness.Services.Statistics
{
    public class RStatisticService : IRStatisticService
    {
        int mantelNumPermutations = 10000;
        private REngine engine;
        public RStatisticService()
        {
            REngine.SetEnvironmentVariables();
            REngine engine = REngine.GetInstance();
            engine.Initialize();
        }

        public void Process()
        {
            // Mantelův test
            engine.Evaluate(@"
            library(vegan)
            set.seed(42)
            
            # Simulace dat pro Mantel test
            mat1 <- matrix(runif(25), nrow = 5)
            mat2 <- matrix(runif(25), nrow = 5)
            mantel_result <- mantel(mat1, mat2)
        ");
            Console.WriteLine("Mantel statistic: " + engine.Evaluate("mantel_result$statistic").AsNumeric().First());

            // PERMANOVA
            engine.Evaluate(@"
            # Simulace dat pro PERMANOVA
            group <- factor(c('A', 'A', 'B', 'B', 'C'))
            dist_mat <- vegdist(matrix(runif(25), nrow = 5))
            permanova_result <- adonis2(dist_mat ~ group)
        ");
            Console.WriteLine("PERMANOVA p-value: " + engine.Evaluate("permanova_result$`Pr(>F)`[1]").AsNumeric().First());

            // MDS
            engine.Evaluate(@"
            # Simulace dat pro MDS
            dist_mat <- dist(matrix(runif(25), nrow = 5))
            mds_result <- cmdscale(dist_mat, k = 2)
        ");
            var mdsCoordinates = engine.Evaluate("mds_result").AsNumericMatrix();
            Console.WriteLine("MDS Coordinates:");
            for (int i = 0; i < mdsCoordinates.RowCount; i++)
            {
                Console.WriteLine($"Point {i + 1}: ({mdsCoordinates[i, 0]}, {mdsCoordinates[i, 1]})");
            }

            // Ukončení R prostředí
            engine.Dispose();
        }

        #region MantelTest

        public async Task<MantelResponse> ProcessMantel(MantelRequest request)
        {
            double mantelR = MantelTest(request.BaseMatrix, request.DerivedMatrix, mantelNumPermutations, out double pValue);
            return new MantelResponse() { CorelationCoeficient = mantelR, PValue = pValue, PermutationCount = mantelNumPermutations };
        }

        double MantelTest(double[,] matrixA, double[,] matrixB, int permutations, out double pValue)
        {
            int size = matrixA.GetLength(0);
            double[] flatA = FlattenMatrix(matrixA);
            double[] flatB = FlattenMatrix(matrixB);

            // Korelace mezi původními maticemi
            double originalCorrelation = PearsonCorrelation(flatA, flatB);

            // Permutace a výpočet náhodných korelací
            int greaterCount = 0;
            object lockObject = new object(); // Zámek pro synchronizaci sdílené proměnné
            ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

            Parallel.For(0, permutations, (i, state) =>
            {
                Random localRandom = threadLocalRandom.Value; // Unikátní Random pro každé vlákno
                double[] shuffledB = ShuffleArray(flatB, localRandom);
                double permutedCorrelation = PearsonCorrelation(flatA, shuffledB);

                lock (lockObject) // Atomická inkrementace
                {
                    if (permutedCorrelation >= originalCorrelation)
                        greaterCount++;
                }
            });

            // Výpočet p-hodnoty (počet permutací s větší korelací / celkový počet)
            pValue = (double)greaterCount / permutations;
            return originalCorrelation;
        }

        double[] FlattenMatrix(double[,] matrix)
        {
            int size = matrix.GetLength(0);
            double[] flatArray = new double[size * (size - 1) / 2];
            int index = 0;
            for (int i = 0; i < size; i++)
                for (int j = i + 1; j < size; j++)
                    flatArray[index++] = matrix[i, j];

            return flatArray;
        }

        double PearsonCorrelation(double[] arrayA, double[] arrayB)
        {
            double meanA = arrayA.Average();
            double meanB = arrayB.Average();

            double covariance = arrayA.Zip(arrayB, (a, b) => (a - meanA) * (b - meanB)).Sum();
            double stdDevA = Math.Sqrt(arrayA.Sum(x => Math.Pow(x - meanA, 2)));
            double stdDevB = Math.Sqrt(arrayB.Sum(x => Math.Pow(x - meanB, 2)));

            return covariance / (stdDevA * stdDevB);
        }

        double[] ShuffleArray(double[] array, Random random)
        {
            double[] shuffled = array.ToArray();
            for (int i = shuffled.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            return shuffled;
        }

        #endregion

        #region PermanovaTest

        public async Task<PermanovaResponse> ProcessPermanova(PermanovaRequest request)
        {
            // Inicializace R prostředí
            REngine.SetEnvironmentVariables();
            REngine engine = REngine.GetInstance();
            engine.Initialize();

            // 1. Vytvoření matice v C#
            double[,] matrixData = {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 },
            { 7.0, 8.0, 9.0 }
        };

            // 2. Předání matice do R
            NumericMatrix rMatrix = engine.CreateNumericMatrix(matrixData);
            engine.SetSymbol("myMatrix", rMatrix);

            // 3. Operace v R
            engine.Evaluate(@"
            print('Matrix in R:')
            print(myMatrix)
            result <- myMatrix * 2  # Například násobení každého prvku dvěma
        ");

            // 4. Získání výsledku zpět do C#
            NumericMatrix resultMatrix = engine.GetSymbol("result").AsNumericMatrix();
            Console.WriteLine("Result Matrix:");
            for (int i = 0; i < resultMatrix.RowCount; i++)
            {
                for (int j = 0; j < resultMatrix.ColumnCount; j++)
                {
                    Console.Write(resultMatrix[i, j] + " ");
                }
                Console.WriteLine();
            }

            // Ukončení R prostředí
            engine.Dispose();
            return new PermanovaResponse();
        }

        #endregion
    }
}
