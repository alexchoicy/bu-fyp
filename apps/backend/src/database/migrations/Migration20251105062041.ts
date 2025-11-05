import { Migration } from '@mikro-orm/migrations';

export class Migration20251105062041 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table "group_course" ("group" bigint not null, "Course" bigint not null, "required" boolean not null, constraint "group_course_pkey" primary key ("group", "Course"));`);

    this.addSql(`alter table "group_course" add constraint "group_course_group_foreign" foreign key ("group") references "group_entity" ("id") on update cascade;`);
    this.addSql(`alter table "group_course" add constraint "group_course_Course_foreign" foreign key ("Course") references "course" ("id") on update cascade;`);

    this.addSql(`drop table if exists "Group_course" cascade;`);
  }

  override async down(): Promise<void> {
    this.addSql(`create table "Group_course" ("group" bigint not null, "Course" bigint not null, constraint "Group_course_pkey" primary key ("group", "Course"));`);

    this.addSql(`alter table "Group_course" add constraint "Group_course_group_foreign" foreign key ("group") references "group_entity" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Group_course" add constraint "Group_course_Course_foreign" foreign key ("Course") references "course" ("id") on update cascade on delete cascade;`);

    this.addSql(`drop table if exists "group_course" cascade;`);
  }

}
