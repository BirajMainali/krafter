namespace Krafter;

public class KrafterAttribute : Attribute
{
    public KrafterAttribute(
        bool includeInInsert = false,
        bool includeInUpdate = false,
        bool includeInInput = false,
        bool includeInOutput = false)
    {
        IncludeInInsert = includeInInsert;
        IncludeInUpdate = includeInUpdate;
        IncludeInInput = includeInInput;
        IncludeInOutput = includeInOutput;
    }

    public bool IncludeInInsert { get; set; }
    public bool IncludeInUpdate { get; set; }
    public bool IncludeInInput { get; set; }
    public bool IncludeInOutput { get; set; }
}