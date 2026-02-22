using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Programmes;

public class ItemCompletionStatus
{
    public required int groupCourseID { get; set; }
    public int? CourseID { get; set; }
    public int? CodeID { get; set; }
    public bool IsCompleted { get; set; }
    public int CreditsUsed { get; set; }
}

public class CategoryCompletionStatus
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public RuleNode? RuleNode { get; set; }
    public string? Notes { get; set; }
    public int MinCredit { get; set; } = 0;
    public int Priority { get; set; } = 0;
    public IList<ItemCompletionStatus> Items { get; set; } = new List<ItemCompletionStatus>();
    public bool IsCompleted { get; set; }
    public int UsedCredits { get; set; }
    public IList<int> UsedCourseIds { get; set; } = new List<int>();
}

public interface IEvaluateRule
{
    Task<IList<CategoryCompletionStatus>> CheckUserCompletion(string userId);
    List<int> GetUnfulfilledGroupsForCategory(RuleNode rules, HashSet<int> completedGroupIds);
}
public class EvaluateRule : IEvaluateRule
{

    private readonly AppDbContext _context;

    public EvaluateRule(AppDbContext context)
    {
        _context = context;
    }
    // what is this, what was I writing, my brain is dying, so tired
    public async Task<IList<CategoryCompletionStatus>> CheckUserCompletion(string userId)
    {
        var studentProgramme = await _context.StudentProgrammes
            .AsNoTracking()
            .Include(p => p.ProgrammeVersion)
            .ThenInclude(pv => pv.ProgrammeCategories)
            .ThenInclude(pc => pc.Category)
            .ThenInclude(c => c.CategoryGroups)
            .Where(p => p.StudentId == userId)
            .FirstOrDefaultAsync();

        if (studentProgramme == null)
            return new List<CategoryCompletionStatus>();

        var programmeCategories = studentProgramme.ProgrammeVersion.ProgrammeCategories
            .OrderByDescending(pc => pc.Category.Priority)
            .ToList();

        var studentCourses = await _context.StudentCourses
            .AsNoTracking()
            .Include(sc => sc.Course)
            .Where(sc => sc.StudentId == userId)
            .ToListAsync();

        var usedCourseIdsAcrossCategories = new HashSet<int>();

        var results = new List<CategoryCompletionStatus>();
        foreach (var programmeCategory in programmeCategories)
        {
            var category = programmeCategory.Category;

            var groupIds = category.CategoryGroups
                .Select(cg => cg.GroupId)
                .Distinct()
                .ToList();

            //groupCourse has CodeId and CourseId
            var groupCourses = await _context.GroupCourses
                .AsNoTracking()
                .Where(gc => groupIds.Contains(gc.GroupId))
                .ToListAsync();

            //filter used by prev categories
            var availableStudentCourses = studentCourses
                .Where(sc => !usedCourseIdsAcrossCategories.Contains(sc.CourseId))
                .ToList();

            var categoryStatus = CheckCategoryCompletion(category, groupCourses, availableStudentCourses);

            // mark used course IDs across categories based on Items
            foreach (var item in categoryStatus.Items.Where(i => i.IsCompleted && i.CourseID.HasValue))
            {
                usedCourseIdsAcrossCategories.Add(item.CourseID.Value);
            }

            results.Add(categoryStatus);
        }

        return results;
    }

    private CategoryCompletionStatus CheckCategoryCompletion(
        Category category,
        IList<GroupCourse> groupCourses,
        IList<StudentCourse> studentCourses)
    {
        var usedStudentCourseIds = new HashSet<int>();
        var evalResult = EvaluateRuleNode(
            category.Rules,
            category.MinCredit,
            groupCourses,
            studentCourses,
            usedStudentCourseIds);

        bool rulesSatisfied = evalResult.IsSatisfied;
        //throw everything to frontend and do things there Zzzzz
        return new CategoryCompletionStatus
        {
            Id = category.Id,
            Name = category.Name,
            Notes = category.Notes,
            MinCredit = category.MinCredit,
            Priority = category.Priority,
            RuleNode = category.Rules,
            Items = evalResult.Items,
            IsCompleted = rulesSatisfied,
            UsedCredits = evalResult.UsedCredits,
            //i am lazy let frontend request another request
            UsedCourseIds = usedStudentCourseIds.ToList()
        };
    }

    public class RuleEvalResult
    {
        public bool IsSatisfied { get; set; }
        public List<ItemCompletionStatus> Items { get; set; } = new();
        public int UsedCredits { get; set; }
    }

    private RuleEvalResult EvaluateRuleNode(
        RuleNode node,
        int category_minCredit,
        IList<GroupCourse> allGroupCourses,
        IList<StudentCourse> allStudentCourses,
        HashSet<int> usedStudentCourseIds)
    {
        if (node is RuleRuleNode ruleNode)
        {
            return EvaluateCompositeRuleNode(ruleNode, category_minCredit, allGroupCourses, allStudentCourses, usedStudentCourseIds);
        }

        if (node is GroupRuleNode groupNode)
        {
            return EvaluateGroupRuleNode(groupNode, allGroupCourses, allStudentCourses, usedStudentCourseIds);
        }

        if (node is FreeElectiveRuleNode freeElectiveNode)
        {
            return EvaluateFreeElectiveRuleNode(category_minCredit, freeElectiveNode, allGroupCourses, allStudentCourses, usedStudentCourseIds);
        }

        throw new InvalidOperationException($"Unsupported RuleNode type: {node.GetType().Name}");
    }


    private RuleEvalResult EvaluateCompositeRuleNode(
        RuleRuleNode ruleNode,
        int category_minCredit,
        IList<GroupCourse> allGroupCourses,
        IList<StudentCourse> allStudentCourses,
        HashSet<int> usedStudentCourseIds)
    {
        var result = new RuleEvalResult();

        if (ruleNode.Children == null || ruleNode.Children.Count == 0)
        {
            result.IsSatisfied = true;
            result.UsedCredits = 0;
            return result;
        }

        if (ruleNode.Operator == RuleOperator.And)
        {
            bool allSatisfied = true;

            foreach (var child in ruleNode.Children)
            {
                var childResult = EvaluateRuleNode(child, category_minCredit, allGroupCourses, allStudentCourses, usedStudentCourseIds);

                result.Items.AddRange(childResult.Items);
                result.UsedCredits += childResult.UsedCredits;

                if (!childResult.IsSatisfied)
                    allSatisfied = false;
            }

            result.IsSatisfied = allSatisfied;
        }
        else // Any
        {
            foreach (var child in ruleNode.Children)
            {
                var tempUsedStudentCourseIds = new HashSet<int>(usedStudentCourseIds);

                var childResult = EvaluateRuleNode(child, category_minCredit, allGroupCourses, allStudentCourses, tempUsedStudentCourseIds);

                if (childResult.IsSatisfied)
                {
                    usedStudentCourseIds.Clear();
                    usedStudentCourseIds.UnionWith(tempUsedStudentCourseIds);
                    result.IsSatisfied = true;
                    result.Items.AddRange(childResult.Items);
                    result.UsedCredits += childResult.UsedCredits;
                    return result;
                }

                if (child == ruleNode.Children.Last())
                {
                    // So I want to retrun the last evaluated result even if not satisfied
                    result.Items.AddRange(childResult.Items);
                    result.UsedCredits += childResult.UsedCredits;
                }
            }

            result.IsSatisfied = false;
        }

        return result;
    }

    private sealed class GroupCourseMatch
    {
        public GroupCourse GroupCourse { get; set; } = null!;
        public List<StudentCourse> Matches { get; set; } = new();
    }

    // Check if a course matches a group course's CourseId or CodeId
    private bool MatchesGroupCourse(GroupCourse gc, StudentCourse sc)
    {
        if (gc.CourseId.HasValue && gc.CourseId.Value == sc.CourseId)
            return true;

        if (gc.CodeId.HasValue && sc.Course.CodeId == gc.CodeId)
            return true;

        return false;
    }

    private RuleEvalResult EvaluateGroupRuleNode(
        GroupRuleNode groupNode,
        IList<GroupCourse> allGroupCourses,
        IList<StudentCourse> allStudentCourses,
        HashSet<int> usedStudentCourseIds)
    {
        var result = new RuleEvalResult();

        var groupEntries = allGroupCourses
            .Where(g => g.GroupId == groupNode.GroupID)
            .ToList();

        var matchesByGroupCourse = groupEntries
            .Select(gc => new GroupCourseMatch
            {
                GroupCourse = gc,
                Matches = allStudentCourses
                    .Where(sc =>
                        !usedStudentCourseIds.Contains(sc.CourseId) &&
                        (GradeUtility.IsPassing(sc.Grade ?? Grade.NA) || sc.Status == StudentCourseStatus.Exemption) &&
                        MatchesGroupCourse(gc, sc))
                    .ToList()
            })
            .ToList();

        switch (groupNode.CourseSelectionMode)
        {
            case CourseSelectionMode.AllOf:
                EvaluateAllOf(matchesByGroupCourse, result, usedStudentCourseIds);
                break;
            case CourseSelectionMode.OneOf:
                EvaluateOneOf(matchesByGroupCourse, result, usedStudentCourseIds);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(groupNode.CourseSelectionMode),
                    groupNode.CourseSelectionMode,
                    null);
        }
        return result;
    }

    private void EvaluateAllOf(
        IList<GroupCourseMatch> matchesByGroupCourse,
        RuleEvalResult result,
        HashSet<int> usedStudentCourseIds)
    {
        // It will return true if the group has no courses to match, but it is fine right now.
        bool allSatisfied = true;

        foreach (var entry in matchesByGroupCourse)
        {
            var gc = entry.GroupCourse;
            var matches = entry.Matches
                .Where(sc => !usedStudentCourseIds.Contains(sc.CourseId))
                .ToList();

            var status = new ItemCompletionStatus
            {
                groupCourseID = gc.GroupId,
                CourseID = gc.CourseId,
                CodeID = gc.CodeId
            };

            if (matches.Any())
            {
                var sc = matches.First();
                usedStudentCourseIds.Add(sc.CourseId);

                status.IsCompleted = true;
                status.CreditsUsed = sc.Course.Credit;

                result.UsedCredits += status.CreditsUsed;
            }
            else
            {
                status.IsCompleted = false;
                allSatisfied = false;
            }

            result.Items.Add(status);
        }
        result.IsSatisfied = allSatisfied;
    }

    private void EvaluateOneOf(
        IList<GroupCourseMatch> matchesByGroupCourse,
        RuleEvalResult result,
        HashSet<int> usedStudentCourseIds)
    {
        bool anySatisfied = false;

        foreach (var entry in matchesByGroupCourse)
        {
            var gc = entry.GroupCourse;
            var matches = entry.Matches
                .Where(sc => !usedStudentCourseIds.Contains(sc.CourseId))
                .ToList();

            var status = new ItemCompletionStatus
            {
                groupCourseID = gc.GroupId,
                CourseID = gc.CourseId,
                CodeID = gc.CodeId
            };

            if (matches.Any())
            {
                var sc = matches.First();
                usedStudentCourseIds.Add(sc.CourseId);

                status.IsCompleted = true;
                status.CreditsUsed = sc.Course.Credit;

                result.UsedCredits += status.CreditsUsed;
                result.Items.Add(status);
                result.IsSatisfied = true;
                return;
            }
            else
            {
                status.IsCompleted = false;
            }

            result.Items.Add(status);
        }

        result.IsSatisfied = anySatisfied;
    }

    private RuleEvalResult EvaluateFreeElectiveRuleNode(
        int Category_minCredit,
        FreeElectiveRuleNode freeElectiveNode,
        IList<GroupCourse> allGroupCourses,
        IList<StudentCourse> allStudentCourses,
        HashSet<int> usedStudentCourseIds)
    {
        var result = new RuleEvalResult();

        var groupEntries = allGroupCourses
            .Where(g => g.GroupId == freeElectiveNode.GroupID)
            .ToList();

        var matchesByGroupCourse = groupEntries
            .Select(gc => new GroupCourseMatch
            {
                GroupCourse = gc,
                Matches = allStudentCourses
                    .Where(sc =>
                        !usedStudentCourseIds.Contains(sc.CourseId) &&
                        (GradeUtility.IsPassing(sc.Grade ?? Grade.NA) || sc.Status == StudentCourseStatus.Exemption) &&
                        MatchesGroupCourse(gc, sc))
                    .ToList()
            })
            .ToList();

        int totalCredits = 0;
        foreach (var entry in matchesByGroupCourse)
        {
            var gc = entry.GroupCourse;
            var matches = entry.Matches
                .Where(sc => !usedStudentCourseIds.Contains(sc.CourseId))
                .ToList();

            var status = new ItemCompletionStatus
            {
                groupCourseID = gc.GroupId,
                CourseID = gc.CourseId,
                CodeID = gc.CodeId
            };

            if (matches.Any())
            {
                var sc = matches.First();
                usedStudentCourseIds.Add(sc.CourseId);

                status.IsCompleted = true;
                status.CreditsUsed = sc.Course.Credit;

                totalCredits += status.CreditsUsed;
                result.UsedCredits += status.CreditsUsed;
            }


            result.Items.Add(status);
        }

        if (totalCredits >= Category_minCredit)
        {
            result.IsSatisfied = true;
        }
        else
        {
            result.IsSatisfied = false;
        }
        return result;
    }

    // lets pretend it work, suppose it should return the bestPath.
    public List<int> GetUnfulfilledGroupsForCategory(RuleNode rules, HashSet<int> completedGroupIds)
    {
        var allGroupsInPath = new List<int>();

        // Find the best matching path and collect all group IDs from it
        CollectGroupsFromBestPath(rules, completedGroupIds, allGroupsInPath);

        // Filter to only return groups that haven't been completed
        return allGroupsInPath
            .Where(groupId => !completedGroupIds.Contains(groupId))
            .Distinct()
            .ToList();
    }

    private void CollectGroupsFromBestPath(
        RuleNode node,
        HashSet<int> completedGroupIds,
        List<int> collectedGroups)
    {
        if (node is GroupRuleNode groupNode)
        {
            Console.WriteLine($"Visiting GroupRuleNode with GroupID: {groupNode.GroupID}");
            collectedGroups.Add(groupNode.GroupID);
        }
        else if (node is RuleRuleNode ruleNode)
        {
            if (ruleNode.Children == null || ruleNode.Children.Count == 0)
                return;

            if (ruleNode.Operator == RuleOperator.And)
            {
                // For And operator, collect from all children
                foreach (var child in ruleNode.Children)
                {
                    CollectGroupsFromBestPath(child, completedGroupIds, collectedGroups);
                }
            }
            else // Any operator
            {
                // Find the path with the most completed groups (best match)
                var bestPath = FindBestMatchingPath(ruleNode.Children, completedGroupIds);
                Console.WriteLine($"Best path operator: {ruleNode.Operator}, Best path type: {bestPath?.GetType().Name}");
                if (bestPath != null)
                {
                    CollectAllGroups(bestPath, collectedGroups);
                }
            }
        }
        else if (node is FreeElectiveRuleNode freeElectiveNode)
        {
            collectedGroups.Add(freeElectiveNode.GroupID);
        }
    }

    private RuleNode? FindBestMatchingPath(List<RuleNode> paths, HashSet<int> completedGroupIds)
    {
        RuleNode? bestPath = null;
        int bestScore = -1;

        foreach (var path in paths)
        {
            var groupsInPath = new List<int>();
            CollectAllGroups(path, groupsInPath);

            // Score = how many groups in this path are already completed
            int score = groupsInPath.Count(gId => completedGroupIds.Contains(gId));

            if (score > bestScore)
            {
                bestScore = score;
                bestPath = path;
            }
        }

        return bestPath;
    }


    private void CollectAllGroups(RuleNode node, List<int> groups)
    {
        if (node is GroupRuleNode groupNode)
        {
            groups.Add(groupNode.GroupID);
        }
        else if (node is RuleRuleNode ruleNode)
        {
            if (ruleNode.Children != null)
            {
                foreach (var child in ruleNode.Children)
                {
                    CollectAllGroups(child, groups);
                }
            }
        }
        else if (node is FreeElectiveRuleNode freeElectiveNode)
        {
            groups.Add(freeElectiveNode.GroupID);
        }
    }

}
