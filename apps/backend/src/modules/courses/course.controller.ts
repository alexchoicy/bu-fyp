import { Body, Controller, Get, Param, Post } from '@nestjs/common';
import { CourseService } from './course.service.js';
import { CreateCourseDto } from '#types/dto/course.js';

@Controller('courses')
export class CourseController {
	constructor(private readonly courseService: CourseService) {}

	@Get()
	async getAllCourses() {
		return this.courseService.getAllCourses();
	}

	@Get(':id')
	async getCourseById(@Param('id') id: string) {
		return this.courseService.getCourseById(id);
	}

	@Post()
	async createCourse(@Body() courseData: CreateCourseDto) {
		return this.courseService.createCourse(courseData);
	}
}
