import {
	Entity,
	PrimaryKey,
	BigIntType,
	Property,
	JsonType,
	ManyToMany,
	Collection,
} from '@mikro-orm/core';
import { Programme } from './programme.js';
import { GroupEntity } from './group.js';

@Entity()
export class Category {
	@PrimaryKey({ type: BigIntType, autoincrement: true })
	id!: string;

	@Property({ type: 'text' })
	name!: string;

	@Property({ type: JsonType, nullable: true })
	rules?: unknown;

	@Property({ type: 'text', nullable: true })
	notes?: string;

	@Property({ type: 'int' })
	min_credit!: number;

	@ManyToMany(() => Programme, (p) => p.categories, {
		mappedBy: 'categories',
	})
	programmes = new Collection<Programme>(this);

	@ManyToMany(() => GroupEntity, (g) => g.categories, {
		mappedBy: 'group',
	})
	groups = new Collection<GroupEntity>(this);
}
