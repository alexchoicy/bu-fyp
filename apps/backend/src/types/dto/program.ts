import { ProgrammeSchema } from '@fyp/api/program/types';
import { createZodDto } from 'nestjs-zod';

export class CreateProgrammeDto extends createZodDto(ProgrammeSchema) {}
