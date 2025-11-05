import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	ManyToMany,
	Collection,
	ManyToOne,
	OneToMany,
} from '@mikro-orm/core';
import { Category } from './category.js';
import { Course } from './course.js';

@Entity()
export class GroupEntity {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text' })
	name!: string;

	@Property({ type: 'int' })
	min_credit!: number;

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
	// Use a composite PK of (group, Course), or add a surrogate id if preferred.
	@ManyToOne(() => GroupEntity, { fieldName: 'group', primary: true })
	group!: GroupEntity;

	@ManyToOne(() => Course, { fieldName: 'Course', primary: true })
	course!: Course;

	@Property({ type: 'boolean' })
	required!: boolean;
}
