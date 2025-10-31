import { Module, OnModuleInit } from '@nestjs/common';
import { APP_INTERCEPTOR, APP_PIPE } from '@nestjs/core';
import { ZodSerializerInterceptor, ZodValidationPipe } from 'nestjs-zod';
import { ConfigModule } from '@nestjs/config';
import { EnvSchema } from '#config/env.js';

@Module({
	imports: [
		ConfigModule.forRoot({
			isGlobal: true,
			validate: (env) => {
				const res = EnvSchema.safeParse(env);
				if (!res.success) {
					throw new Error(
						'Invalid config:\n' + JSON.stringify(res.error.issues),
					);
				}
				return res.data;
			},
		}),
	],
	controllers: [],
	providers: [
		{
			provide: APP_PIPE,
			useClass: ZodValidationPipe,
		},
		{
			provide: APP_INTERCEPTOR,
			useClass: ZodSerializerInterceptor,
		},
	],
})
export class AppModule implements OnModuleInit {
	constructor() {}
	onModuleInit() {}
}
