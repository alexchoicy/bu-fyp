import { Course } from '#database/entities/course.js';
import { Programme } from '#database/entities/programme.js';
import { StudentCourse, User } from '#database/entities/user.js';
import { CourseCode } from '@fyp/api/course/types';
import { normalizeAcademicYear } from '@fyp/api/course/utils';
import {
	Category as CategoryType,
	ProgrammeCheck,
	ProgrammeCheckCategory,
	ProgrammeListItem,
	RuleNode,
} from '@fyp/api/program/types';
import { checkCategoryCompletion } from '@fyp/api/program/utils';
import { StudentCourseRequest } from '@fyp/api/student/types';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable, NotFoundException } from '@nestjs/common';

@Injectable()
export class UserService {
	constructor(private readonly em: EntityManager) {}

	async checkGraduationRequirements(userId: string) {
		const user = await this.em.findOne(
			User,
			{ id: userId },
			{
				populate: [
					'studiedCourses',
					'studiedCourses.course',
					'programmes',
					'programmes.categories',
					'programmes.categories.groups',
					'programmes.categories.groups.groupCourses',
					'programmes.categories.groups.groupCourses.course',
					'programmes.categories.groups.groupCourses.course.code',
					'programmes.categories.groups.groupCourses.code',
					'programmes.categories.groups.groupCourses.code.tag',
				],
			},
		);

		if (!user) {
			throw new NotFoundException('User not found');
		}
		const studiedCourses = new Map<string, Course>();
		for (const sc of user.studiedCourses) {
			studiedCourses.set(sc.course.id, sc.course);
		}

		const checkedCourses = new Map(studiedCourses);

		const programmes: Array<ProgrammeCheck> = [];

		for (const programme of user.programmes) {
			const categoryResults: Array<ProgrammeCheckCategory> = [];
			for (const category of programme.categories) {
				const groupMap = new Map<
					string,
					{ course: Course[]; tag: CourseCode[] }
				>();

				for (const group of category.groups) {
					for (const gc of group.groupCourses) {
						//whatever zzz
						if (!groupMap.has(group.id)) {
							groupMap.set(group.id, { course: [], tag: [] });
						}
						const groupData = groupMap.get(group.id)!;
						if (gc.course) {
							groupData.course.push(gc.course);
						} else if (gc.code) {
							groupData.tag.push(gc.code);
						}
					}
				}
				const parseToCategory: CategoryType = {
					id: category.id,
					name: category.name,
					ruleTree: category.rules as RuleNode,
					notes: category.notes || undefined,
					min_credit: category.min_credit,
					priority: category.priority,
				};

				const result = checkCategoryCompletion(
					parseToCategory,
					checkedCourses,
					groupMap,
				);
				categoryResults.push(result);
			}
			programmes.push({
				id: programme.id,
				name: programme.name,
				categories: categoryResults,
				completedCourses: Array.from(studiedCourses.values()),
			});
		}
		return programmes;
	}

	// async checkGraduationRequirements(userId: string) {
	// 	const user = await this.em.findOne(
	// 		User,
	// 		{ id: userId },
	// 		{
	// 			populate: [
	// 				'studiedCourses',
	// 				'studiedCourses.course',
	// 				'studiedCourses.course.code',
	// 				'programmes',
	// 				'programmes.categories',
	// 				'programmes.categories.groups',
	// 				'programmes.categories.groups.groupCourses',
	// 				'programmes.categories.groups.groupCourses.course',
	// 				'programmes.categories.groups.groupCourses.course.code',
	// 				'programmes.categories.groups.groupCourses.code',
	// 			],
	// 		},
	// 	);

	// 	if (!user) {
	// 		throw new NotFoundException('User not found');
	// 	}

	// 	const studentCourseMap = new Map<
	// 		string,
	// 		{
	// 			status: StudentCourseStatus;
	// 			grade: string | null;
	// 			course: Course;
	// 		}
	// 	>();

	// 	for (const studentCourse of user.studiedCourses) {
	// 		if (!studentCourse.course?.id) {
	// 			continue;
	// 		}

	// 		studentCourseMap.set(studentCourse.course.id, {
	// 			status: studentCourse.status,
	// 			grade: studentCourse.grade ?? null,
	// 			course: studentCourse.course,
	// 		});
	// 	}

	// 	const programmeResults: ProgrammeRequirement[] = [];

	// 	for (const programme of user.programmes) {
	// 		const programmeCategories = programme.categories
	// 			.getItems()
	// 			.slice()
	// 			.sort((a, b) => b.priority - a.priority);

	// 		const categorySummaries: ProgrammeRequirementCategory[] = [];

	// 		let programmeTotalCredits = 0;
	// 		let programmeCompletedCredits = 0;

	// 		for (const category of programmeCategories) {
	// 			const groupMap = new Map<
	// 				string,
	// 				{ courses: Course[]; tagIds: string[] }
	// 			>();

	// 			for (const group of category.groups) {
	// 				for (const groupCourse of group.groupCourses) {
	// 					const entry = groupMap.get(group.id) ?? {
	// 						courses: [],
	// 						tagIds: [],
	// 					};

	// 					if (groupCourse.course) {
	// 						entry.courses.push(groupCourse.course);
	// 					}

	// 					if (groupCourse.code) {
	// 						entry.tagIds.push(groupCourse.code.id);
	// 					}

	// 					groupMap.set(group.id, entry);
	// 				}
	// 			}

	// 			const courseEntries = new Map<
	// 				string,
	// 				ProgrammeRequirementCourse
	// 			>();
	// 			const tagIds = new Set<string>();

	// 			for (const { courses, tagIds: ids } of groupMap.values()) {
	// 				for (const course of courses) {
	// 					if (!course.id) {
	// 						continue;
	// 					}
	// 					if (!courseEntries.has(course.id)) {
	// 						const credits = course.credit ?? 0;
	// 						courseEntries.set(course.id, {
	// 							id: course.id,
	// 							name: course.name,
	// 							credits,
	// 							status: 'not-started',
	// 							grade: null,
	// 							courseNumber: course.courseNumber,
	// 							tag: course.code?.tag ?? '',
	// 						});
	// 					}
	// 				}

	// 				for (const tagId of ids) {
	// 					tagIds.add(tagId);
	// 				}
	// 			}
	// 			let categoryCompletedCredits = 0;
	// 			const matchedCourseIds = new Set<string>();

	// 			for (const [courseId, entry] of courseEntries) {
	// 				const studentCourse = studentCourseMap.get(courseId);
	// 				if (!studentCourse) {
	// 					continue;
	// 				}

	// 				const status = this.mapRequirementStatus(
	// 					studentCourse.status,
	// 				);
	// 				entry.status = status;
	// 				entry.grade = studentCourse.grade;

	// 				if (status === 'completed') {
	// 					categoryCompletedCredits += entry.credits;
	// 				}

	// 				matchedCourseIds.add(courseId);
	// 			}

	// 			for (const [courseId, studentCourse] of studentCourseMap) {
	// 				if (matchedCourseIds.has(courseId)) {
	// 					continue;
	// 				}

	// 				const codeId = studentCourse.course.code?.id;
	// 				if (!codeId || !tagIds.has(codeId)) {
	// 					continue;
	// 				}

	// 				const credits = studentCourse.course.credit ?? 0;
	// 				const status = this.mapRequirementStatus(
	// 					studentCourse.status,
	// 				);

	// 				courseEntries.set(courseId, {
	// 					id: courseId,
	// 					tag: studentCourse.course.code?.tag ?? '',
	// 					courseNumber: studentCourse.course.courseNumber,
	// 					name: studentCourse.course.name,
	// 					credits,
	// 					status,
	// 					grade: studentCourse.grade,
	// 				});

	// 				if (status === 'completed') {
	// 					categoryCompletedCredits += credits;
	// 				}

	// 				matchedCourseIds.add(courseId);
	// 			}

	// 			const entryCreditsSum = Array.from(
	// 				courseEntries.values(),
	// 			).reduce((total, course) => total + course.credits, 0);

	// 			const categoryTotalCredits =
	// 				category.min_credit > 0
	// 					? category.min_credit
	// 					: entryCreditsSum;

	// 			categoryCompletedCredits = Math.min(
	// 				categoryCompletedCredits,
	// 				categoryTotalCredits,
	// 			);

	// 			const statusPriority: Record<RequirementCourseStatus, number> =
	// 				{
	// 					completed: 0,
	// 					'in-progress': 1,
	// 					'not-started': 2,
	// 				};

	// 			const sortedCourses = Array.from(courseEntries.values()).sort(
	// 				(a, b) => {
	// 					const statusDiff =
	// 						statusPriority[a.status] - statusPriority[b.status];
	// 					if (statusDiff !== 0) {
	// 						return statusDiff;
	// 					}

	// 					return a.tag.localeCompare(b.tag);
	// 				},
	// 			);

	// 			categorySummaries.push({
	// 				id: category.id,
	// 				name: category.name,
	// 				rules: category.rules as RuleNode,
	// 				description: category.notes ?? null,
	// 				totalCredits: categoryTotalCredits,
	// 				completedCredits: categoryCompletedCredits,
	// 				courses: sortedCourses,
	// 			});

	// 			programmeTotalCredits += categoryTotalCredits;
	// 			programmeCompletedCredits += categoryCompletedCredits;
	// 		}
	// 		programmeCompletedCredits = Math.min(
	// 			programmeCompletedCredits,
	// 			programmeTotalCredits,
	// 		);

	// 		const progressPercentage =
	// 			programmeTotalCredits > 0
	// 				? (programmeCompletedCredits / programmeTotalCredits) * 100
	// 				: 0;

	// 		programmeResults.push({
	// 			programmeId: programme.id,
	// 			programmeName: programme.name,
	// 			totalCredits: programmeTotalCredits,
	// 			completedCredits: programmeCompletedCredits,
	// 			progressPercentage,
	// 			categories: categorySummaries,
	// 		});
	// 	}
	// 	return programmeResults;
	// }

	async getCoursesTaken(userId: string): Promise<StudentCourse[]> {
		const user = await this.em.find(
			StudentCourse,
			{
				student: { id: userId },
			},
			{
				populate: ['course', 'course.code'],
			},
		);

		if (!user) {
			throw new NotFoundException('User not found');
		}

		return user;
	}

	async setCoursesTaken(userId: string, data: StudentCourseRequest) {
		const user = await this.em.findOne(User, { id: userId });
		if (!user) {
			throw new NotFoundException('User not found');
		}

		const course = await this.em.findOne(Course, { id: data.courseID });
		if (!course) {
			throw new NotFoundException('Course not found');
		}

		let studentCourse = await this.em.findOne(StudentCourse, {
			student: { id: userId },
			course: { id: data.courseID },
		});

		if (!studentCourse) {
			studentCourse = new StudentCourse();
			studentCourse.student = user;
			studentCourse.course = course;
		}

		studentCourse.status = data.status;
		studentCourse.grade = data.grade;
		studentCourse.term = data.term;
		studentCourse.year = normalizeAcademicYear(data.year);

		await this.em.persistAndFlush(studentCourse);

		return studentCourse;
	}

	async getUserProgrammes(userId: string): Promise<ProgrammeListItem[]> {
		const user = await this.em.findOne(
			User,
			{ id: userId },
			{ populate: ['programmes'] },
		);

		if (!user) {
			throw new NotFoundException('User not found');
		}

		return user.programmes
			.getItems()
			.map((programme) => this.mapProgramme(programme));
	}

	async addProgramme(
		userId: string,
		programmeId: string,
	): Promise<ProgrammeListItem> {
		const user = await this.em.findOne(
			User,
			{ id: userId },
			{ populate: ['programmes'] },
		);

		if (!user) {
			throw new NotFoundException('User not found');
		}

		const programme = await this.em.findOne(Programme, { id: programmeId });

		if (!programme) {
			throw new NotFoundException('Programme not found');
		}

		if (!user.programmes.contains(programme)) {
			user.programmes.add(programme);
			await this.em.flush();
		}

		return this.mapProgramme(programme);
	}

	private mapProgramme(programme: Programme): ProgrammeListItem {
		return {
			id: programme.id,
			name: programme.name,
			version: programme.version,
		};
	}
}
