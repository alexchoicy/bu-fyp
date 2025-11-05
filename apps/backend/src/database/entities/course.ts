import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	OneToMany,
	Collection,
	ManyToOne,
	ManyToMany,
} from '@mikro-orm/core';
import { GroupCourse } from './group.js';

@Entity()
export class Code {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text' })
	tag!: string;

	@Property({ type: 'text' })
	name!: string;

	@OneToMany(() => Course, (c) => c.code)
	courses = new Collection<Course>(this);
}

@Entity()
export class Course {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text' })
	name!: string;

	@Property({ type: 'int' })
	credit!: number;

	@Property({ type: 'boolean' })
	is_active!: boolean;

	@ManyToOne(() => Code, { fieldName: 'Code', nullable: true })
	code?: Code;

	@OneToMany(() => GroupCourse, (gc) => gc.course)
	groupLinks = new Collection<GroupCourse>(this);

	@ManyToMany(() => Course, 'isPrerequisiteFor', {
		pivotTable: 'Course_pre_req',
		joinColumn: 'course', // current course
		inverseJoinColumn: 'req_course', // required/prereq course
		owner: true,
	})
	prerequisites = new Collection<Course>(this);

	@ManyToMany(() => Course, (c) => c.prerequisites, {
		pivotTable: 'Course_pre_req',
		joinColumn: 'req_course',
		inverseJoinColumn: 'course',
	})
	isPrerequisiteFor = new Collection<Course>(this);

	@ManyToMany(() => Course, 'isAntiRequisiteFor', {
		pivotTable: 'Course_anti_req',
		joinColumn: 'course',
		inverseJoinColumn: 'exclude_course',
		owner: true,
	})
	antiRequisites = new Collection<Course>(this);

	@ManyToMany(() => Course, (c) => c.antiRequisites, {
		pivotTable: 'Course_anti_req',
		joinColumn: 'exclude_course',
		inverseJoinColumn: 'course',
	})
	isAntiRequisiteFor = new Collection<Course>(this);
}
