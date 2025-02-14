namespace Orthologist.Bussiness.Services.Helpers
{
    public static class PhyloTreeHelper
    {
        public static string BuildNewickTree(double[,] matrix, string[] labels)
        {

            int n = labels.Length;
            var nodes = new List<string>(labels);
            matrix = NormalizeDistanceMatrix(matrix);
            var branchLengths = new Dictionary<string, double>();

            // Initialize branch lengths
            foreach (var label in labels)
            {
                branchLengths[label] = 0.0;
            }

            while (nodes.Count > 2)
            {
                int size = nodes.Count;

                // Calculate Q matrix
                double[,] qMatrix = new double[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (i == j) continue;
                        qMatrix[i, j] = (size - 2) * matrix[i, j] -
                                        matrix.GetRowSum(i) -
                                        matrix.GetRowSum(j);
                    }
                }

                // Find pair (i, j) with minimum Q value
                int minI = 0, minJ = 1;
                double minQ = double.MaxValue;
                for (int i = 0; i < size; i++)
                {
                    for (int j = i + 1; j < size; j++)
                    {
                        if (qMatrix[i, j] < minQ)
                        {
                            minQ = qMatrix[i, j];
                            minI = i;
                            minJ = j;
                        }
                    }
                }

                // Calculate new node distances
                double d1 = 0.5 * (matrix[minI, minJ] +
                                   (matrix.GetRowSum(minI) - matrix.GetRowSum(minJ)) / (size - 2));
                double d2 = matrix[minI, minJ] - d1;

                // Create new node
                string newNode = $"({nodes[minI]}:{d1:F3},{nodes[minJ]}:{d2:F3})";

                // Update branch lengths
                branchLengths[newNode] = 0.0;

                // Update matrix
                var newMatrix = new double[size - 1, size - 1];
                for (int i = 0, newI = 0; i < size; i++)
                {
                    if (i == minI || i == minJ) continue;
                    for (int j = 0, newJ = 0; j < size; j++)
                    {
                        if (j == minI || j == minJ) continue;

                        newMatrix[newI, newJ] = matrix[i, j];
                        newJ++;
                    }
                    newI++;
                }

                // Add distances for the new node
                for (int k = 0, index = 0; k < size; k++)
                {
                    if (k == minI || k == minJ) continue;

                    double newDistance = 0.5 * (matrix[minI, k] + matrix[minJ, k] - matrix[minI, minJ]);
                    newMatrix[newMatrix.GetLength(0) - 1, index] = newDistance;
                    newMatrix[index, newMatrix.GetLength(0) - 1] = newDistance;

                    index++;
                }

                // Replace old nodes with the new node
                nodes[minJ] = newNode;
                nodes.RemoveAt(minI);
                matrix = newMatrix;
            }

            // Add the final two nodes
            string finalTree = $"({nodes[0]}:{matrix[0, 1]:F3},{nodes[1]}:{matrix[0, 1]:F3});";
            return finalTree;
        }

        private static double GetRowSum(this double[,] matrix, int rowIndex)
        {
            int size = matrix.GetLength(0);
            double sum = 0.0;
            for (int j = 0; j < size; j++)
            {
                sum += matrix[rowIndex, j];
            }
            return sum;
        }

        public static double[,] NormalizeDistanceMatrix(double[,] similarityMatrix)
        {
            int n = similarityMatrix.GetLength(0);
            double maxScore = double.MinValue;
            double minScore = double.MaxValue;

            // Najdi maximální a minimální skóre
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    maxScore = Math.Max(maxScore, similarityMatrix[i, j]);
                    minScore = Math.Min(minScore, similarityMatrix[i, j]);
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
                    distanceMatrix[i, j] = (maxScore - similarityMatrix[i, j]) / range;
                }
            }

            return distanceMatrix;
        }
    }
}

