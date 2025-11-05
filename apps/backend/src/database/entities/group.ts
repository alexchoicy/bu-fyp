import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	ManyToMany,
	Collection,
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

	@ManyToMany(() => Course, (c) => c.groups, {
		pivotTable: 'Group_course',
		joinColumn: 'group',
		inverseJoinColumn: 'Course',
		owner: true,
	})
	courses = new Collection<Course>(this);
}
