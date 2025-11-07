import { Module } from '@nestjs/common';
import { GroupController } from './groups.controller.js';
import { GroupService } from './groups.service.js';

@Module({
	controllers: [GroupController],
	providers: [GroupService],
})
export class GroupModule {}
