import { CreateProgrammeDto } from '#types/dto/program.js';
import type { RuleNode } from '@fyp/api/program/types';
import { Body, Controller, Get, Param, Post } from '@nestjs/common';
import { TagService } from './tag.service.js';

@Controller('tags')
export class TagController {
	constructor(private readonly tagService: TagService) {}

	@Get()
	async getAllTags() {
		return this.tagService.getTags();
	}
}
