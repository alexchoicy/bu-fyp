import { Code, Course } from '#database/entities/course.js';
import { GroupCourse, GroupEntity } from '#database/entities/group.js';
import { Course as CourseType } from '@fyp/api/course/types';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable, NotFoundException } from '@nestjs/common';

@Injectable()
export class CourseService {
	constructor(private readonly em: EntityManager) {}

	courseDTOBuilder(course: Course): CourseType {
		return {
			id: course.id,
			name: course.name,
			credit: course.credit,
			courseNumber: course.courseNumber,
			description: course.description,
			is_active: course.is_active,
			code: course.code
				? {
						id: course.code.id,
						tag: course.code.tag,
						name: course.code.name,
					}
				: undefined,
			prerequisites: course.prerequisites.getItems().map((prereq) => ({
				id: prereq.id,
				name: prereq.name,
				credit: prereq.credit,
				courseNumber: prereq.courseNumber,
				is_active: prereq.is_active,
				prerequisites: [],
				antiRequisites: [],
				groups: [],
			})),
			antiRequisites: course.antiRequisites.getItems().map((antiReq) => ({
				id: antiReq.id,
				name: antiReq.name,
				credit: antiReq.credit,
				courseNumber: antiReq.courseNumber,
				is_active: antiReq.is_active,
				prerequisites: [],
				antiRequisites: [],
				groups: [],
			})),
			groups: course.groupLinks.getItems().map((link) => ({
				id: link.group.id,
				name: link.group.name,
			})),
		};
	}

	async getAllCourses(): Promise<CourseType[]> {
		const courses = await this.em.find(
			Course,
			{},
			{
				populate: [
					'code',
					'antiRequisites',
					'prerequisites',
					'groupLinks',
				],
			},
		);

		return courses.map((course) => this.courseDTOBuilder(course));
	}

	async getCourseById(id: string): Promise<CourseType> {
		const course = await this.em.findOne(
			Course,
			{ id },
			{
				populate: [
					'code',
					'antiRequisites',
					'prerequisites',
					'groupLinks',
				],
			},
		);
		if (!course) throw new NotFoundException('Course not found');
		return this.courseDTOBuilder(course);
	}

	async createCourse(courseData: CourseType) {
		const code = await this.em.findOne(Code, { id: courseData.code?.id });
		if (!code) throw new NotFoundException('Code not found');
		const existingCourse = await this.em.findOne(Course, {
			courseNumber: courseData.courseNumber,
			code: code,
		});
		if (existingCourse)
			throw new NotFoundException(
				'Course with this code and number already exists',
			);
		const course = this.em.create(Course, {
			name: courseData.name,
			credit: courseData.credit,
			courseNumber: courseData.courseNumber,
			is_active: courseData.is_active,
			code,
		});

		await this.em.persistAndFlush(course);

		for (const linkGroup of courseData.groups) {
			const group = await this.em.findOne(GroupEntity, {
				id: linkGroup.id,
			});
			if (!group) continue;
			const groupCourse = this.em.create(GroupCourse, {
				group,
				course,
			});
			await this.em.persistAndFlush(groupCourse);
		}
	}
}
