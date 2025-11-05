import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	ManyToMany,
	Collection,
} from '@mikro-orm/core';
import { Category } from './category.js';

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
}
