using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Backend.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GroupRuleNode), "group")]
[JsonDerivedType(typeof(RuleRuleNode), "rule")]
[JsonDerivedType(typeof(FreeElectiveRuleNode), "free_elective")]
public abstract class RuleNode
{
    [JsonIgnore]   
    public abstract string Type { get; }
}

public class GroupRuleNode : RuleNode
{
    [JsonIgnore] 
    public override string Type => "group";

    public required int GroupID { get; set; }

    public CourseSelectionMode? CourseSelectionMode { get; set; }
}

public class RuleRuleNode : RuleNode
{
    [JsonIgnore] 
    public override string Type => "rule";

    public RuleOperator? Operator { get; set; }

    public List<RuleNode>? Children { get; set; }
}

public class FreeElectiveRuleNode : RuleNode
{
    [JsonIgnore] 
    public override string Type => "free_elective";

    public int? MinCredits { get; set; }
    public required int GroupID { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseSelectionMode
{
    OneOf,
    AllOf
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleOperator
{
    And,
    Any
}
