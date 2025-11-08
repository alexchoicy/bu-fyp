import { Controller, Get, Param } from '@nestjs/common';
import { UserService } from './user.service.js';

@Controller('users')
export class UserController {
	constructor(private readonly userService: UserService) {}

	@Get(':id/check')
	async checkUser(@Param('id') id: string) {}
}
