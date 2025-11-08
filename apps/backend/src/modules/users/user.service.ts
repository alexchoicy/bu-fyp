import { User } from '#database/entities/user.js';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';

@Injectable()
export class UserService {
	constructor(private readonly em: EntityManager) {}
}
