import { Body, Controller, Get, Param, Post } from '@nestjs/common';
import { UserService } from './user.service.js';
import type { StudentCourseRequest } from '@fyp/api/student/types';
import { ProgrammeAssignmentDto } from '#types/dto/program.js';

@Controller('users')
export class UserController {
	constructor(private readonly userService: UserService) {}

	@Get(':id/check')
	async checkUser(@Param('id') id: string) {
		return this.userService.checkGraduationRequirements(id);
	}

	@Get(':id/courses')
	async getUserCourses(@Param('id') id: string) {
		return this.userService.getCoursesTaken(id);
	}

	@Post(':id/courses')
	async setUserCourses(
		@Param('id') id: string,
		@Body() data: StudentCourseRequest,
	) {
		return this.userService.setCoursesTaken(id, data);
	}

	@Get(':id/programmes')
	async getUserProgrammes(@Param('id') id: string) {
		return this.userService.getUserProgrammes(id);
	}

	@Post(':id/programmes')
	async addProgramme(
		@Param('id') id: string,
		@Body() data: ProgrammeAssignmentDto,
	) {
		return this.userService.addProgramme(id, data.programmeId);
	}
}
