import {
	Code,
	Course,
	CourseMeeting,
	CourseSection,
} from '#database/entities/course.js';
import { GroupCourse, GroupEntity } from '#database/entities/group.js';
import {
	Course as CourseType,
	CreateCourseSection,
} from '@fyp/api/course/types';
import { normalizeAcademicYear } from '@fyp/api/course/utils';
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
				sections: [],
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
				sections: [],
			})),
			groups: course.groupLinks.getItems().map((link) => ({
				id: link.group.id,
				name: link.group.name,
			})),
			sections: course.sections.getItems().map((section) => ({
				id: section.id,
				capacity: section.position,
				meetings: section.meetings.getItems().map((meeting) => ({
					id: meeting.id,
					day: meeting.day,
					startTime: meeting.startTime,
					endTime: meeting.endTime,
					location: meeting.location,
					sectionType: meeting.sectionType,
				})),
				term: section.term,
				position: section.position,
				year: normalizeAcademicYear(section.year),
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
					'sections',
					'sections.meetings',
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
					'sections',
					'sections.meetings',
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

	async getCourseSections(courseId: string) {
		const sections = await this.em.find(
			CourseSection,
			{
				course: { id: courseId },
			},
			{
				populate: ['meetings'],
			},
		);
		return sections;
	}

	async createCourseSection(
		courseId: string,
		sectionData: CreateCourseSection,
	) {
		const course = await this.em.findOne(Course, { id: courseId });
		if (!course) throw new NotFoundException('Course not found');

		const section = this.em.create(CourseSection, {
			course,
			term: sectionData.term,
			year: normalizeAcademicYear(sectionData.year),
			position: sectionData.position,
		});

		this.em.persist(section);

		for (const meetingData of sectionData.meetings) {
			const meeting = this.em.create(CourseMeeting, {
				courseSection: section,
				sectionType: meetingData.sectionType,
				day: meetingData.day,
				startTime: meetingData.startTime,
				endTime: meetingData.endTime,
				location: meetingData.location,
			});
			section.meetings.add(meeting);
			this.em.persist(meeting);
		}

		await this.em.flush();
		await this.em.populate(section, ['meetings']);

		return section;
	}
}
