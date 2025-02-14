using Orthologist.Bussiness.Classes;

namespace Orthologist.Bussiness.Services.Helpers
{
    public static class DataTablesProvider
    {
        private static readonly Lazy<List<Taxon>> taxons = new(() => SetTaxons());

        private static List<Taxon> SetTaxons()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Taxons.csv");
            return File.ReadAllLines(path).Select(x => new Taxon()
            {
                Id = x.Split(',', StringSplitOptions.RemoveEmptyEntries)[0],
                Name = x.Split(',', StringSplitOptions.RemoveEmptyEntries)[1]
            }).ToList();
        }

        public static string? GetTaxonName(string id) => taxons.Value.FirstOrDefault(x => x.Id == id)?.Name ?? string.Empty;
        public static string? GetTaxonId(string name) => taxons.Value.FirstOrDefault(x => x.Name == name)?.Id ?? string.Empty;
        public static List<Taxon> GetTaxons() => taxons.Value;
    }
}
