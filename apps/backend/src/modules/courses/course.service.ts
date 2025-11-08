import { Code, Course } from '#database/entities/course.js';
import { GroupCourse, GroupEntity } from '#database/entities/group.js';
import { Course as CourseType } from '@fyp/api/course/types';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable, NotFoundException } from '@nestjs/common';

@Injectable()
export class CourseService {
	constructor(private readonly em: EntityManager) {}

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
