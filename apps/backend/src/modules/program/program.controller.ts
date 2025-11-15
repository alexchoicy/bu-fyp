import { CreateProgrammeDto } from '#types/dto/program.js';
import { Body, Controller, Get, Param, Post } from '@nestjs/common';
import { ProgramService } from './program.service.js';

@Controller('program')
export class ProgramController {
	constructor(private readonly programService: ProgramService) {}

	@Get(':id')
	async getProgram(@Param('id') id: string) {
		return this.programService.getProgram(id);
	}

	@Post()
	createProgram(@Body() data: CreateProgrammeDto) {
		return this.programService.createProgram(data);
	}

	@Get()
	async listProgrammes() {
		return this.programService.listProgrammes();
	}
}
