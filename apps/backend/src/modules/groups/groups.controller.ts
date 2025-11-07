import { Body, Controller, Get } from '@nestjs/common';
import { GroupService } from './groups.service.js';

@Controller('groups')
export class GroupController {
	constructor(private readonly groupService: GroupService) {}

	@Get()
	getAllGroups() {
		return this.groupService.getAllGroups();
	}
}
