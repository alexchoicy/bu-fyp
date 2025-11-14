import { Body, Controller, Post, Res } from '@nestjs/common';
import { AuthService } from './auth.service.js';
import { setAuthCookies } from '#utils/auth/cookies.js';
import type { Request, Response } from 'express';
import { Public } from '#decorators/public.decorator.js';
import { ConfigService } from '@nestjs/config/dist/index.js';
import { LoginRequestDTO } from '#types/dto/auth.js';
import { JWTCustomPayload } from '@fyp/api/auth/types';
import { JWKSProvider } from './issuer/jwks.provider.js';

@Controller('auth')
export class AuthController {
	constructor(
		private readonly authService: AuthService,
		private readonly configService: ConfigService,
		private readonly jwksProvider: JWKSProvider,
	) {}

	@Public()
	@Post('user/login')
	async login(
		@Res({ passthrough: true }) res: Response,
		@Body() loginDTO: LoginRequestDTO,
	) {
		const user = await this.authService.validateUser(
			loginDTO.username,
			loginDTO.password,
		);

		const payload: JWTCustomPayload = {
			type: 'access',
			info: {
				uid: user.id.toString(),
				role: 'user',
			},
		};

		const token = await this.jwksProvider.signAccessToken(payload);

		setAuthCookies(res, token, 'localhost');

		return { token };
	}

	@Public()
	@Post('admin/login')
	async adminLogin(
		@Res({ passthrough: true }) res: Response,
		@Body() loginDTO: LoginRequestDTO,
	) {
		const admin = await this.authService.validateAdmin(
			loginDTO.username,
			loginDTO.password,
		);

		const payload: JWTCustomPayload = {
			type: 'access',
			info: {
				uid: admin.id.toString(),
				role: 'admin',
			},
		};

		const token = await this.jwksProvider.signAccessToken(payload);

		setAuthCookies(res, token, 'localhost');

		return { token };
	}
}
