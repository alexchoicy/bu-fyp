import { GroupEntity } from '#database/entities/group.js';
import { Group as GroupType } from '@fyp/api/program/types';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';

@Injectable()
export class GroupService {
	constructor(private readonly em: EntityManager) {}

	async getAllGroups(): Promise<GroupType[]> {
		return this.em.findAll(GroupEntity);
	}
}
