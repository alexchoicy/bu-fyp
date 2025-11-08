import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	ManyToMany,
	Collection,
} from '@mikro-orm/core';
import { Category } from './category.js';
import { User } from './user.js';

@Entity()
export class Programme {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text' })
	name!: string;

	@Property({ type: 'text' })
	version!: string;

	@ManyToMany(() => Category, (c) => c.programmes, {
		pivotTable: 'Programme_Category',
		joinColumn: 'Programme_id',
		inverseJoinColumn: 'Category',
		owner: true,
	})
	categories = new Collection<Category>(this);

	@ManyToMany(() => User, (u: User) => u.programmes, {
		mappedBy: 'programmes',
	})
	students = new Collection<User>(this);
}
