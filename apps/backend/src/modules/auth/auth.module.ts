import { Module } from '@nestjs/common';
import { AuthService } from './auth.service.js';
import { AuthController } from './auth.controller.js';
import { JWKSProvider } from './issuer/jwks.provider.js';

@Module({
	controllers: [AuthController],
	providers: [AuthService, JWKSProvider],
})
export class AuthModule {}
