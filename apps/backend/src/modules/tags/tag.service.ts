import { Code } from '#database/entities/course.js';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';

@Injectable()
export class TagService {
	constructor(private readonly em: EntityManager) {}

	async getTags() {
		// WHY IT CALL CODE NOT TAG????
		return this.em.findAll(Code);
	}
}
