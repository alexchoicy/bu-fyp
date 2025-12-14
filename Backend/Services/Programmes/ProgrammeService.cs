﻿using Backend.Data;
using Backend.Models;
using Backend.Dtos.Programmes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Backend.Services.Programmes;

public interface IProgrammeService
{
    Task<UserProgrammeDetailDto?> GetUserProgrammeDetailAsync(string userId);
}


public class ProgrammeService : IProgrammeService
{
    private readonly AppDbContext _context;

    public ProgrammeService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get the programme detail for a user including all categories, groups, and courses
    /// </summary>
    public async Task<UserProgrammeDetailDto?> GetUserProgrammeDetailAsync(string userId)
    {
        var studentProgramme = await _context.StudentProgrammes
            .AsNoTracking()
            .Include(sp => sp.ProgrammeVersion)
                .ThenInclude(pv => pv.Programme)
            .Include(sp => sp.ProgrammeVersion)
                .ThenInclude(pv => pv.ProgrammeCategories)
                    .ThenInclude(pc => pc.Category)
                        .ThenInclude(c => c.CategoryGroups)
                            .ThenInclude(cg => cg.Group)
                                .ThenInclude(g => g.GroupCourses)
                                    .ThenInclude(gc => gc.Course)
                                        .ThenInclude(c => c.Code)
            .Include(sp => sp.ProgrammeVersion)
                .ThenInclude(pv => pv.ProgrammeCategories)
                    .ThenInclude(pc => pc.Category)
                        .ThenInclude(c => c.CategoryGroups)
                            .ThenInclude(cg => cg.Group)
                                .ThenInclude(g => g.GroupCourses)
                                    .ThenInclude(gc => gc.Code)
            .FirstOrDefaultAsync(sp => sp.StudentId == userId);

        if (studentProgramme == null)
            return null;

        var programmeVersion = studentProgramme.ProgrammeVersion;
        var programme = programmeVersion.Programme;

        var dto = new UserProgrammeDetailDto
        {
            ProgrammeVersionId = programmeVersion.Id,
            ProgrammeId = programme.Id,
            ProgrammeName = programme.Name,
            ProgrammeDescription = programme.Description,
            VersionNumber = programmeVersion.VersionNumber,
            StartYear = programmeVersion.StartYear,
            EndYear = programmeVersion.EndYear,
            IsActive = programmeVersion.IsActive,
            TotalCredits = programmeVersion.TotalCredits,
            CreatedAt = programmeVersion.CreatedAt,
            LastEditedAt = programmeVersion.LastEditedAt,
            Categories = new List<ProgrammeCategoryDetailDto>()
        };

        foreach (var progCategory in programmeVersion.ProgrammeCategories)
        {
            var category = progCategory.Category;
            var categoryDto = new ProgrammeCategoryDetailDto
            {
                CategoryId = category.Id,
                Name = category.Name ?? string.Empty,
                Type = category.Type,
                Notes = category.Notes,
                MinCredit = category.MinCredit,
                Priority = category.Priority,
                RulesJson = category.RulesJson,
                Groups = new List<CategoryGroupDetailDto>()
            };

            foreach (var categoryGroup in category.CategoryGroups)
            {
                var group = categoryGroup.Group;
                var groupDto = new CategoryGroupDetailDto
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    GroupCourses = new List<GroupCourseDetailDto>()
                };

                foreach (var groupCourse in group.GroupCourses)
                {
                    var groupCourseDto = new GroupCourseDetailDto
                    {
                        GroupCourseId = groupCourse.Id
                    };

                    if (groupCourse.Course != null)
                    {
                        groupCourseDto.Course = new SimpleGroupCourseDto
                        {
                            CourseId = groupCourse.Course.Id,
                            Name = groupCourse.Course.Name,
                            CourseNumber = groupCourse.Course.CourseNumber,
                            Credit = groupCourse.Course.Credit,
                            CodeTag = groupCourse.Course.Code.Tag,
                            IsActive = groupCourse.Course.IsActive,
                            Description = groupCourse.Course.Description
                        };
                    }

                    if (groupCourse.Code != null)
                    {
                        groupCourseDto.Code = new SimpleCodeDto
                        {
                            CodeId = groupCourse.Code.Id,
                            Name = groupCourse.Code.Name,
                            Tag = groupCourse.Code.Tag
                        };
                    }

                    groupDto.GroupCourses.Add(groupCourseDto);
                }

                categoryDto.Groups.Add(groupDto);
            }

            dto.Categories.Add(categoryDto);
        }

        return dto;
    }
}
