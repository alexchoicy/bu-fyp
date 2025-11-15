import { ProgrammeSchema } from '@fyp/api/program/types';
import { ProgrammeAssignmentRequestSchema } from '@fyp/api/program/types';
import { createZodDto } from 'nestjs-zod';

export class CreateProgrammeDto extends createZodDto(ProgrammeSchema) {}

export class ProgrammeAssignmentDto extends createZodDto(
	ProgrammeAssignmentRequestSchema,
) {}
