using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Backend.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GroupRuleNode), "group")]
[JsonDerivedType(typeof(RuleRuleNode), "rule")]
public abstract class RuleNode
{
    public abstract string Type { get; }
}

public class GroupRuleNode : RuleNode
{
    public override string Type => "group";

    public required string GroupID { get; set; }

    public CourseSelectionMode? CourseSelectionMode { get; set; }
}

public class RuleRuleNode : RuleNode
{
    public override string Type => "rule";

    public RuleOperator? Operator { get; set; }

    public List<RuleNode>? Children { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseSelectionMode
{
    OneOf,
    AllOf,
    MinCredit
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleOperator
{
    And,
    Any
}
