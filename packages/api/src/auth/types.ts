import { z } from "zod";

export const LoginRequestSchema = z.object({
  username: z.string().min(1),
  password: z.string().min(1),
});

export const UserRolesSchema = z.enum(["user", "admin"]);

export type UserRole = z.infer<typeof UserRolesSchema>;

export interface JWTCustomPayloadUserInfo {
  uid: string;
  role?: UserRole;
}

export interface JWTCustomPayload {
  type: "access";
  info: JWTCustomPayloadUserInfo;
}
