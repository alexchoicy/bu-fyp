import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	ManyToMany,
	Collection,
	ManyToOne,
	OneToMany,
	type Rel,
} from '@mikro-orm/core';
import { Category } from './category.js';
import { Code, Course } from './course.js';

@Entity()
export class GroupEntity {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text' })
	name!: string;

	@ManyToMany(() => Category, (c) => c.groups, {
		pivotTable: 'Category_group',
		joinColumn: 'Group',
		inverseJoinColumn: 'category',
		owner: true,
	})
	categories = new Collection<Category>(this);

	@OneToMany(() => GroupCourse, (gc) => gc.group)
	groupCourses = new Collection<GroupCourse>(this);
}

@Entity()
export class GroupCourse {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@ManyToOne(() => GroupEntity, { fieldName: 'group' })
	group!: Rel<GroupEntity>;

	@ManyToOne(() => Course, { fieldName: 'Course', nullable: true })
	course?: Rel<Course>;
	// I DUNNO
	@ManyToOne(() => Code, { fieldName: 'Code', nullable: true })
	code?: Rel<Code>;
}
