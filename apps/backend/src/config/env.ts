import { z } from 'zod/v4';

export const EnvSchema = z.object({
	DATABASE_URL: z.string().min(1, 'DATABASE_URL is required'),
});

export type EnvConfig = z.infer<typeof EnvSchema>;
