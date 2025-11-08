import { User } from '#database/entities/user.js';
import type { EntityManager } from '@mikro-orm/core';
import { Seeder } from '@mikro-orm/seeder';

import 'dotenv/config';

export class AccountSeeder extends Seeder {
	async run(em: EntityManager): Promise<void> {
		const userExists = await em.findOne(User, { username: 'user' });
		if (!userExists) {
			const user = em.create(User, {
				username: 'user',
				name: 'Default User',
				password: process.env.DEFAULT_USER_PASSWORD || 'password',
			});
			await em.persistAndFlush(user);
		}
	}
}
