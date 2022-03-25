namespace FizzCode.LightWeight.RelationalModel;

public class RelationalColumnPair
{
    public RelationalColumn SourceColumn { get; set; }
    public RelationalColumn TargetColumn { get; set; }

    internal RelationalColumnPair(RelationalColumn sourceColumn, RelationalColumn targetColumn)
    {
        SourceColumn = sourceColumn;
        TargetColumn = targetColumn;
    }
}
