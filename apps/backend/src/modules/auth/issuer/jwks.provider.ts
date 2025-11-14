import { JWTCustomPayload } from '@fyp/api/auth/types';
import { Injectable } from '@nestjs/common';
import { ConfigService } from '@nestjs/config/dist/index.js';
import * as jose from 'jose';

@Injectable()
export class JWKSProvider {
	private secret: Uint8Array;
	private accessExpiry: string | number;

	constructor(private readonly configService: ConfigService) {}

	onModuleInit() {
		const raw = process.env.JWK_SECRET;
		this.secret = Buffer.from(raw!, 'utf8');
	}

	async signAccessToken(payload: JWTCustomPayload): Promise<string> {
		// payload should be a plain object (sub, roles, etc.)
		return await new jose.SignJWT(payload as any)
			.setProtectedHeader({ alg: 'HS256', typ: 'JWT' })
			.setIssuedAt()
			.sign(this.secret);
	}

	async verifyToken(
		token: string,
	): Promise<jose.JWTVerifyResult<JWTCustomPayload>> {
		return await jose.jwtVerify<JWTCustomPayload>(token, this.secret);
	}
}
