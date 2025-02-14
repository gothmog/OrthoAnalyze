namespace Orthologist.Bussiness.Classes
{
    public class TaxItem
    {
        public TaxItem() { }
        public TaxItem(TaxItem item) 
        {
            key = item.key;
            name = item.name;
            Alias = item.Alias;
            Count = item.Count;
            Parent = item.Parent;
        }

        public TaxItem(TaxItem item, bool selected)
        {
            key = item.key;
            name = item.name;
            Alias = item.Alias;
            Count = item.Count;
            Parent = item.Parent;
            Selected = selected;
        }


        public string key { get; set; }
        public string name { get; set; }
        public string? Alias { get; set; }
        public int Count { get; set; }
        public string Parent { get; set; }
        public bool Selected { get; set; }
        public bool Main { get; set; } = true;
        public List<TaxItem> children { get; set; }
    }

    public class TaxTree
    {
        public List<TaxItem> Data { get; set; }
    }
}
