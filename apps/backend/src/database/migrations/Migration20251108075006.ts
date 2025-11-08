import { Migration } from '@mikro-orm/migrations';

export class Migration20251108075006 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`alter table "group_course" drop constraint "group_course_Course_foreign";`);

    this.addSql(`alter table "course" add column "course_number" int not null, add column "description" text null;`);

    this.addSql(`alter table "group_course" drop constraint "group_course_pkey";`);
    this.addSql(`alter table "group_course" drop column "required";`);

    this.addSql(`alter table "group_course" add column "id" bigserial not null, add column "Code" bigint null;`);
    this.addSql(`alter table "group_course" alter column "Course" type bigint using ("Course"::bigint);`);
    this.addSql(`alter table "group_course" alter column "Course" drop not null;`);
    this.addSql(`alter table "group_course" add constraint "group_course_Code_foreign" foreign key ("Code") references "code" ("id") on update cascade on delete set null;`);
    this.addSql(`alter table "group_course" add constraint "group_course_Course_foreign" foreign key ("Course") references "course" ("id") on update cascade on delete set null;`);
    this.addSql(`alter table "group_course" add constraint "group_course_pkey" primary key ("id");`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table "group_course" drop constraint "group_course_Code_foreign";`);
    this.addSql(`alter table "group_course" drop constraint "group_course_Course_foreign";`);

    this.addSql(`alter table "course" drop column "course_number", drop column "description";`);

    this.addSql(`alter table "group_course" drop constraint "group_course_pkey";`);
    this.addSql(`alter table "group_course" drop column "id", drop column "Code";`);

    this.addSql(`alter table "group_course" add column "required" boolean not null;`);
    this.addSql(`alter table "group_course" alter column "Course" type bigint using ("Course"::bigint);`);
    this.addSql(`alter table "group_course" alter column "Course" set not null;`);
    this.addSql(`alter table "group_course" add constraint "group_course_Course_foreign" foreign key ("Course") references "course" ("id") on update cascade;`);
    this.addSql(`alter table "group_course" add constraint "group_course_pkey" primary key ("group", "Course");`);
  }

}
