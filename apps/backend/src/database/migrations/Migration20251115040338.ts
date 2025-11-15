import { Migration } from '@mikro-orm/migrations';

export class Migration20251115040338 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table "admin" ("id" bigserial primary key, "username" text not null, "name" text not null, "password" varchar(255) not null);`);
    this.addSql(`alter table "admin" add constraint "admin_username_unique" unique ("username");`);

    this.addSql(`create table "category" ("id" bigserial primary key, "name" text not null, "rules" jsonb null, "notes" text null, "min_credit" int not null, "priority" int not null);`);

    this.addSql(`create table "code" ("id" bigserial primary key, "tag" text not null, "name" text not null);`);
    this.addSql(`alter table "code" add constraint "code_tag_unique" unique ("tag");`);

    this.addSql(`create table "course" ("id" bigserial primary key, "name" text not null, "credit" int not null, "is_active" boolean not null, "Code" bigint null, "course_number" int not null, "description" text null);`);

    this.addSql(`create table "Course_anti_req" ("course" bigint not null, "exclude_course" bigint not null, constraint "Course_anti_req_pkey" primary key ("course", "exclude_course"));`);

    this.addSql(`create table "Course_pre_req" ("course" bigint not null, "req_course" bigint not null, constraint "Course_pre_req_pkey" primary key ("course", "req_course"));`);

    this.addSql(`create table "course_section" ("id" bigserial primary key, "courseID" bigint not null, "term" text not null, "year" text not null, "position" int not null);`);

    this.addSql(`create table "course_meeting" ("id" bigserial primary key, "courseSectionID" bigint not null, "section_type" text not null, "day" text not null, "start_time" text not null, "end_time" text not null, "location" text not null);`);

    this.addSql(`create table "group_entity" ("id" bigserial primary key, "name" text not null);`);

    this.addSql(`create table "group_course" ("id" bigserial primary key, "group" bigint not null, "Course" bigint null, "Code" bigint null);`);

    this.addSql(`create table "Category_group" ("Group" bigint not null, "category" bigint not null, constraint "Category_group_pkey" primary key ("Group", "category"));`);

    this.addSql(`create table "programme" ("id" bigserial primary key, "name" text not null, "version" text not null);`);

    this.addSql(`create table "Programme_Category" ("Programme_id" bigint not null, "Category" bigint not null, constraint "Programme_Category_pkey" primary key ("Programme_id", "Category"));`);

    this.addSql(`create table "User" ("id" bigserial primary key, "username" text not null, "name" text not null, "password" varchar(255) not null, "Cache_Programme" jsonb null);`);
    this.addSql(`alter table "User" add constraint "User_username_unique" unique ("username");`);

    this.addSql(`create table "student_course" ("id" bigserial primary key, "student_id" bigint not null, "course_id" bigint not null, "status" text not null, "grade" text not null, "term" text not null, "year" text not null, "course_section_id" bigint not null);`);

    this.addSql(`create table "student_programme" ("student_id" bigint not null, "programme_id" bigint not null, constraint "student_programme_pkey" primary key ("student_id", "programme_id"));`);

    this.addSql(`alter table "course" add constraint "course_Code_foreign" foreign key ("Code") references "code" ("id") on update cascade on delete set null;`);

    this.addSql(`alter table "Course_anti_req" add constraint "Course_anti_req_course_foreign" foreign key ("course") references "course" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Course_anti_req" add constraint "Course_anti_req_exclude_course_foreign" foreign key ("exclude_course") references "course" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "Course_pre_req" add constraint "Course_pre_req_course_foreign" foreign key ("course") references "course" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Course_pre_req" add constraint "Course_pre_req_req_course_foreign" foreign key ("req_course") references "course" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "course_section" add constraint "course_section_courseID_foreign" foreign key ("courseID") references "course" ("id") on update cascade;`);

    this.addSql(`alter table "course_meeting" add constraint "course_meeting_courseSectionID_foreign" foreign key ("courseSectionID") references "course_section" ("id") on update cascade;`);

    this.addSql(`alter table "group_course" add constraint "group_course_group_foreign" foreign key ("group") references "group_entity" ("id") on update cascade;`);
    this.addSql(`alter table "group_course" add constraint "group_course_Course_foreign" foreign key ("Course") references "course" ("id") on update cascade on delete set null;`);
    this.addSql(`alter table "group_course" add constraint "group_course_Code_foreign" foreign key ("Code") references "code" ("id") on update cascade on delete set null;`);

    this.addSql(`alter table "Category_group" add constraint "Category_group_Group_foreign" foreign key ("Group") references "group_entity" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Category_group" add constraint "Category_group_category_foreign" foreign key ("category") references "category" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "Programme_Category" add constraint "Programme_Category_Programme_id_foreign" foreign key ("Programme_id") references "programme" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Programme_Category" add constraint "Programme_Category_Category_foreign" foreign key ("Category") references "category" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "student_course" add constraint "student_course_student_id_foreign" foreign key ("student_id") references "User" ("id") on update cascade;`);
    this.addSql(`alter table "student_course" add constraint "student_course_course_id_foreign" foreign key ("course_id") references "course" ("id") on update cascade;`);
    this.addSql(`alter table "student_course" add constraint "student_course_course_section_id_foreign" foreign key ("course_section_id") references "course_section" ("id") on update cascade;`);

    this.addSql(`alter table "student_programme" add constraint "student_programme_student_id_foreign" foreign key ("student_id") references "User" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "student_programme" add constraint "student_programme_programme_id_foreign" foreign key ("programme_id") references "programme" ("id") on update cascade on delete cascade;`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table "Category_group" drop constraint "Category_group_category_foreign";`);

    this.addSql(`alter table "Programme_Category" drop constraint "Programme_Category_Category_foreign";`);

    this.addSql(`alter table "course" drop constraint "course_Code_foreign";`);

    this.addSql(`alter table "group_course" drop constraint "group_course_Code_foreign";`);

    this.addSql(`alter table "Course_anti_req" drop constraint "Course_anti_req_course_foreign";`);

    this.addSql(`alter table "Course_anti_req" drop constraint "Course_anti_req_exclude_course_foreign";`);

    this.addSql(`alter table "Course_pre_req" drop constraint "Course_pre_req_course_foreign";`);

    this.addSql(`alter table "Course_pre_req" drop constraint "Course_pre_req_req_course_foreign";`);

    this.addSql(`alter table "course_section" drop constraint "course_section_courseID_foreign";`);

    this.addSql(`alter table "group_course" drop constraint "group_course_Course_foreign";`);

    this.addSql(`alter table "student_course" drop constraint "student_course_course_id_foreign";`);

    this.addSql(`alter table "course_meeting" drop constraint "course_meeting_courseSectionID_foreign";`);

    this.addSql(`alter table "student_course" drop constraint "student_course_course_section_id_foreign";`);

    this.addSql(`alter table "group_course" drop constraint "group_course_group_foreign";`);

    this.addSql(`alter table "Category_group" drop constraint "Category_group_Group_foreign";`);

    this.addSql(`alter table "Programme_Category" drop constraint "Programme_Category_Programme_id_foreign";`);

    this.addSql(`alter table "student_programme" drop constraint "student_programme_programme_id_foreign";`);

    this.addSql(`alter table "student_course" drop constraint "student_course_student_id_foreign";`);

    this.addSql(`alter table "student_programme" drop constraint "student_programme_student_id_foreign";`);

    this.addSql(`drop table if exists "admin" cascade;`);

    this.addSql(`drop table if exists "category" cascade;`);

    this.addSql(`drop table if exists "code" cascade;`);

    this.addSql(`drop table if exists "course" cascade;`);

    this.addSql(`drop table if exists "Course_anti_req" cascade;`);

    this.addSql(`drop table if exists "Course_pre_req" cascade;`);

    this.addSql(`drop table if exists "course_section" cascade;`);

    this.addSql(`drop table if exists "course_meeting" cascade;`);

    this.addSql(`drop table if exists "group_entity" cascade;`);

    this.addSql(`drop table if exists "group_course" cascade;`);

    this.addSql(`drop table if exists "Category_group" cascade;`);

    this.addSql(`drop table if exists "programme" cascade;`);

    this.addSql(`drop table if exists "Programme_Category" cascade;`);

    this.addSql(`drop table if exists "User" cascade;`);

    this.addSql(`drop table if exists "student_course" cascade;`);

    this.addSql(`drop table if exists "student_programme" cascade;`);
  }

}
