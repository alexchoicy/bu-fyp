import { Admin, User } from '#database/entities/user.js';
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
		const adminExists = await em.findOne(Admin, { username: 'admin' });
		if (!adminExists) {
			const admin = em.create(Admin, {
				username: 'admin',
				name: 'Default Admin',
				password: process.env.DEFAULT_ADMIN_PASSWORD || 'adminpassword',
			});
			await em.persistAndFlush(admin);
		}
	}
}
