import { Migration } from '@mikro-orm/migrations';

export class Migration20251108111244 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table "User" ("id" bigserial primary key, "username" text not null, "name" text not null, "password" varchar(255) not null, "Cache_Programme" jsonb null);`);
    this.addSql(`alter table "User" add constraint "User_username_unique" unique ("username");`);

    this.addSql(`create table "student_course" ("id" bigserial primary key, "student_id" bigint not null, "course_id" bigint not null, "grade" text not null, "term" text not null, "year" text not null);`);

    this.addSql(`create table "student_programme" ("student_id" bigint not null, "programme_id" bigint not null, constraint "student_programme_pkey" primary key ("student_id", "programme_id"));`);

    this.addSql(`alter table "student_course" add constraint "student_course_student_id_foreign" foreign key ("student_id") references "User" ("id") on update cascade;`);
    this.addSql(`alter table "student_course" add constraint "student_course_course_id_foreign" foreign key ("course_id") references "course" ("id") on update cascade;`);

    this.addSql(`alter table "student_programme" add constraint "student_programme_student_id_foreign" foreign key ("student_id") references "User" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "student_programme" add constraint "student_programme_programme_id_foreign" foreign key ("programme_id") references "programme" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "code" add constraint "code_tag_unique" unique ("tag");`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table "student_course" drop constraint "student_course_student_id_foreign";`);

    this.addSql(`alter table "student_programme" drop constraint "student_programme_student_id_foreign";`);

    this.addSql(`drop table if exists "User" cascade;`);

    this.addSql(`drop table if exists "student_course" cascade;`);

    this.addSql(`drop table if exists "student_programme" cascade;`);

    this.addSql(`alter table "code" drop constraint "code_tag_unique";`);
  }

}
