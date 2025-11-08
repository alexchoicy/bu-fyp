import { CourseSchema } from '@fyp/api/course/types';
import { createZodDto } from 'nestjs-zod';

export class CreateCourseDto extends createZodDto(CourseSchema) {}
