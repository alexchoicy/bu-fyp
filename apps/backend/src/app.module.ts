import { Module, OnModuleInit } from '@nestjs/common';
import { APP_INTERCEPTOR, APP_PIPE } from '@nestjs/core';
import { ZodSerializerInterceptor, ZodValidationPipe } from 'nestjs-zod';
import { ConfigModule } from '@nestjs/config';
import { EnvSchema } from '#config/env.js';

import { MikroOrmModule } from '@mikro-orm/nestjs';
import { MikroORM } from '@mikro-orm/postgresql';
import dbConfig from './mikro-orm.config.js';
import { ProgramModule } from '#modules/program/program.module.js';
import { BaseController } from '#modules/base.controller.js';
import { GroupModule } from '#modules/groups/groups.module.js';
import { TagModule } from '#modules/tags/tag.module.js';
import { CourseModule } from '#modules/courses/course.module.js';
import { UserModule } from '#modules/users/user.module.js';
import { AccountSeeder } from '#database/seeder/accountSeeder.js';
import { AuthModule } from '#modules/auth/auth.module.js';

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
		MikroOrmModule.forRoot(dbConfig),
		ProgramModule,
		GroupModule,
		TagModule,
		CourseModule,
		UserModule,
		AuthModule,
	],
	controllers: [BaseController],
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
	constructor(private readonly orm: MikroORM) {}
	async onModuleInit() {
		await this.orm.getMigrator().up();
		await this.orm.seeder.seed(AccountSeeder);
	}
}
