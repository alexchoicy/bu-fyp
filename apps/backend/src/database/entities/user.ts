import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	JsonType,
	ManyToMany,
	Collection,
	OneToMany,
	ManyToOne,
	BeforeCreate,
	BeforeUpdate,
	type EventArgs,
} from '@mikro-orm/core';
import { Programme } from './programme.js';
import { Course, CourseSection } from './course.js';
import { hash, verify } from 'argon2';
import type { StudentCourseStatus, TermCode } from '@fyp/api/static/types';

@Entity({ tableName: 'User' })
export class User {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text', unique: true })
	username!: string;

	@Property({ type: 'text' })
	name!: string;

	@Property({ hidden: true, lazy: true })
	password: string;

	@Property({ type: JsonType, fieldName: 'Cache_Programme', nullable: true })
	cacheProgramme?: unknown;

	@ManyToMany(() => Programme, (p) => p.students, {
		pivotTable: 'student_programme',
		joinColumn: 'student_id',
		inverseJoinColumn: 'programme_id',
		owner: true,
	})
	programmes = new Collection<Programme>(this);

	@OneToMany(() => StudentCourse, (sc) => sc.student)
	studiedCourses = new Collection<StudentCourse>(this);

	@BeforeUpdate()
	@BeforeCreate()
	async hashPassword(args: EventArgs<User>) {
		const password = args.changeSet?.payload.password;
		if (password) {
			this.password = await hash(password);
		}
	}

	async verifyPassword(password: string) {
		return await verify(this.password, password);
	}
}

@Entity()
export class Admin {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text', unique: true })
	username!: string;

	@Property({ type: 'text' })
	name!: string;

	@Property({ hidden: true, lazy: true })
	password: string;

	@BeforeUpdate()
	@BeforeCreate()
	async hashPassword(args: EventArgs<Admin>) {
		const password = args.changeSet?.payload.password;
		if (password) {
			this.password = await hash(password);
		}
	}

	async verifyPassword(password: string) {
		return await verify(this.password, password);
	}
}

@Entity({ tableName: 'student_course' })
export class StudentCourse {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@ManyToOne(() => User, { fieldName: 'student_id' })
	student!: User;

	@ManyToOne(() => Course, { fieldName: 'course_id' })
	course!: Course;

	@Property({ type: 'text' })
	status!: StudentCourseStatus;

	@Property({ type: 'text' })
	grade!: string;

	@Property({ type: 'text' })
	term!: TermCode;

	@Property({ type: 'text' })
	year!: string;

	@ManyToOne(() => CourseSection)
	courseSection!: CourseSection | null;
}
