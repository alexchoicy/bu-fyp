import type { JWTCustomPayload } from "@fyp/api/auth/types";
import type { JWTPayload } from "jose";
import { jwtVerify } from "jose";

const secret = process.env.NUXT_JWK_SECRET;

export async function verifyJWKS(token: string) {
  const { payload } = await jwtVerify(token, Buffer.from(secret!, "utf8"));
  return payload as JWTPayload & JWTCustomPayload;
}
