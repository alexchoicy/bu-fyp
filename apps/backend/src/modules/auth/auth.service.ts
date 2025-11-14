import { Injectable, NotFoundException } from '@nestjs/common';
import { EntityManager } from '@mikro-orm/postgresql';
import { Admin, User } from '#database/entities/user.js';

@Injectable()
export class AuthService {
	constructor(private readonly em: EntityManager) {}

	async validateUser(username: string, password: string) {
		const user = await this.em.findOne(
			User,
			{ username },
			{ populate: ['password'] },
		);
		if (!user) {
			throw new NotFoundException('User not found');
		}

		const isPasswordValid = await user.verifyPassword(password);

		if (!isPasswordValid) {
			throw new NotFoundException('Invalid credentials');
		}

		return user;
	}

	async validateAdmin(username: string, password: string) {
		const admin = await this.em.findOne(
			Admin,
			{ username },
			{ populate: ['password'] },
		);
		if (!admin) {
			throw new NotFoundException('Admin not found');
		}

		const isPasswordValid = await admin.verifyPassword(password);

		if (!isPasswordValid) {
			throw new NotFoundException('Invalid credentials');
		}

		return admin;
	}
}
