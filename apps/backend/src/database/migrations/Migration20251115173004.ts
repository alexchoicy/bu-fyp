import { Migration } from '@mikro-orm/migrations';

export class Migration20251115173004 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`alter table "student_course" drop constraint "student_course_course_section_id_foreign";`);

    this.addSql(`alter table "student_course" alter column "course_section_id" type bigint using ("course_section_id"::bigint);`);
    this.addSql(`alter table "student_course" alter column "course_section_id" drop not null;`);
    this.addSql(`alter table "student_course" add constraint "student_course_course_section_id_foreign" foreign key ("course_section_id") references "course_section" ("id") on update cascade on delete set null;`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table "student_course" drop constraint "student_course_course_section_id_foreign";`);

    this.addSql(`alter table "student_course" alter column "course_section_id" type bigint using ("course_section_id"::bigint);`);
    this.addSql(`alter table "student_course" alter column "course_section_id" set not null;`);
    this.addSql(`alter table "student_course" add constraint "student_course_course_section_id_foreign" foreign key ("course_section_id") references "course_section" ("id") on update cascade;`);
  }

}
