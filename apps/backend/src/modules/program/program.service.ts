import { Programme } from '@fyp/api/program/types';
import { EntityManager } from '@mikro-orm/postgresql';
import { Injectable } from '@nestjs/common';

@Injectable()
export class ProgramService {
	constructor(private readonly em: EntityManager) {}

	createProgram(data: Programme) {}
}
