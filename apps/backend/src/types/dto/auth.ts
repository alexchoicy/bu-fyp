import { LoginRequestSchema } from '@fyp/api/auth/types';
import { createZodDto } from 'nestjs-zod';

export class LoginRequestDTO extends createZodDto(LoginRequestSchema) {}
