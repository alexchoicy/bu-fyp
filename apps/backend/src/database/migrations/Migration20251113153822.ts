import { Migration } from '@mikro-orm/migrations';

export class Migration20251113153822 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table "admin" ("id" bigserial primary key, "username" text not null, "name" text not null, "password" varchar(255) not null);`);
    this.addSql(`alter table "admin" add constraint "admin_username_unique" unique ("username");`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "admin" cascade;`);
  }

}
