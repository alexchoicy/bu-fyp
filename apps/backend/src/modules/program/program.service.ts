import { Category } from '#database/entities/category.js';
import { GroupEntity } from '#database/entities/group.js';
import { Programme } from '#database/entities/programme.js';
import { Programme as ProgrammeType } from '@fyp/api/program/types';
import { getAllGroupsFromCategory } from '@fyp/api/program/utils';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';

@Injectable()
export class ProgramService {
	constructor(private readonly em: EntityManager) {}

	async createProgram(data: ProgrammeType) {
		const existingProgram = await this.em.findOne(Programme, {
			name: data.name,
		});
		if (existingProgram) {
			throw new Error('Program with the same name already exists');
		}

		const program = this.em.create(Programme, {
			name: data.name,
			version: '1',
		});
		await this.em.persistAndFlush(program);

		for (const categoryData of data.categories) {
			const category = this.em.create(Category, {
				name: categoryData.name,
				min_credit: categoryData.min_credit,
				priority: categoryData.priority,
				rules: categoryData.ruleTree,
				notes: categoryData.notes,
			});
			program.categories.add(category);

			const groups = getAllGroupsFromCategory(categoryData.ruleTree);
			for (const groupID of groups) {
				const group = await this.em.findOneOrFail(GroupEntity, {
					id: groupID,
				});
				category.groups.add(group);
			}

			await this.em.persistAndFlush(category);
		}

		await this.em.persistAndFlush(program);
	}

	async getProgram(id: string) {
		const program = await this.em.findOne(
			Programme,
			{ id },
			{
				populate: [
					'categories',
					'categories.groups',
					'categories.groups.groupCourses',
				],
			},
		);
		return program;
	}
}
