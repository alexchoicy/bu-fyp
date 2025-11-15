import { Course } from '#database/entities/course.js';
import { StudentCourse, User } from '#database/entities/user.js';
import { CourseCode } from '@fyp/api/course/types';
import { normalizeAcademicYear } from '@fyp/api/course/utils';
import {
	Category as CategoryType,
	ProgrammeCheck,
	ProgrammeCheckCategory,
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
					'programmes',
					'programmes.categories',
					'programmes.categories.groups',
					'programmes.categories.groups.groupCourses',
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
}
