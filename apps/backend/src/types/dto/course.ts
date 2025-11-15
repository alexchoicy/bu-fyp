import { CourseSchema, CreateCourseSectionSchema } from '@fyp/api/course/types';
import { createZodDto } from 'nestjs-zod';

export class CreateCourseDto extends createZodDto(CourseSchema) {}

export class CreateCourseSectionDto extends createZodDto(
	CreateCourseSectionSchema,
) {}
