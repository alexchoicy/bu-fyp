import { Migration } from '@mikro-orm/migrations';

export class Migration20251105055423 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table "category" ("id" bigserial primary key, "name" text not null, "rules" jsonb null, "notes" text null, "min_credit" int not null);`);

    this.addSql(`create table "code" ("id" bigserial primary key, "tag" text not null, "name" text not null);`);

    this.addSql(`create table "course" ("id" bigserial primary key, "name" text not null, "credit" int not null, "is_active" boolean not null, "Code" bigint null);`);

    this.addSql(`create table "Course_anti_req" ("course" bigint not null, "exclude_course" bigint not null, constraint "Course_anti_req_pkey" primary key ("course", "exclude_course"));`);

    this.addSql(`create table "Course_pre_req" ("course" bigint not null, "req_course" bigint not null, constraint "Course_pre_req_pkey" primary key ("course", "req_course"));`);

    this.addSql(`create table "group_entity" ("id" bigserial primary key, "name" text not null, "min_credit" int not null);`);

    this.addSql(`create table "Group_course" ("group" bigint not null, "Course" bigint not null, constraint "Group_course_pkey" primary key ("group", "Course"));`);

    this.addSql(`create table "Category_group" ("Group" bigint not null, "category" bigint not null, constraint "Category_group_pkey" primary key ("Group", "category"));`);

    this.addSql(`create table "programme" ("id" bigserial primary key, "name" text not null, "version" text not null);`);

    this.addSql(`create table "Programme_Category" ("Programme_id" bigint not null, "Category" bigint not null, constraint "Programme_Category_pkey" primary key ("Programme_id", "Category"));`);

    this.addSql(`alter table "course" add constraint "course_Code_foreign" foreign key ("Code") references "code" ("id") on update cascade on delete set null;`);

    this.addSql(`alter table "Course_anti_req" add constraint "Course_anti_req_course_foreign" foreign key ("course") references "course" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Course_anti_req" add constraint "Course_anti_req_exclude_course_foreign" foreign key ("exclude_course") references "course" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "Course_pre_req" add constraint "Course_pre_req_course_foreign" foreign key ("course") references "course" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Course_pre_req" add constraint "Course_pre_req_req_course_foreign" foreign key ("req_course") references "course" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "Group_course" add constraint "Group_course_group_foreign" foreign key ("group") references "group_entity" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Group_course" add constraint "Group_course_Course_foreign" foreign key ("Course") references "course" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "Category_group" add constraint "Category_group_Group_foreign" foreign key ("Group") references "group_entity" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Category_group" add constraint "Category_group_category_foreign" foreign key ("category") references "category" ("id") on update cascade on delete cascade;`);

    this.addSql(`alter table "Programme_Category" add constraint "Programme_Category_Programme_id_foreign" foreign key ("Programme_id") references "programme" ("id") on update cascade on delete cascade;`);
    this.addSql(`alter table "Programme_Category" add constraint "Programme_Category_Category_foreign" foreign key ("Category") references "category" ("id") on update cascade on delete cascade;`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table "Category_group" drop constraint "Category_group_category_foreign";`);

    this.addSql(`alter table "Programme_Category" drop constraint "Programme_Category_Category_foreign";`);

    this.addSql(`alter table "course" drop constraint "course_Code_foreign";`);

    this.addSql(`alter table "Course_anti_req" drop constraint "Course_anti_req_course_foreign";`);

    this.addSql(`alter table "Course_anti_req" drop constraint "Course_anti_req_exclude_course_foreign";`);

    this.addSql(`alter table "Course_pre_req" drop constraint "Course_pre_req_course_foreign";`);

    this.addSql(`alter table "Course_pre_req" drop constraint "Course_pre_req_req_course_foreign";`);

    this.addSql(`alter table "Group_course" drop constraint "Group_course_Course_foreign";`);

    this.addSql(`alter table "Group_course" drop constraint "Group_course_group_foreign";`);

    this.addSql(`alter table "Category_group" drop constraint "Category_group_Group_foreign";`);

    this.addSql(`alter table "Programme_Category" drop constraint "Programme_Category_Programme_id_foreign";`);

    this.addSql(`drop table if exists "category" cascade;`);

    this.addSql(`drop table if exists "code" cascade;`);

    this.addSql(`drop table if exists "course" cascade;`);

    this.addSql(`drop table if exists "Course_anti_req" cascade;`);

    this.addSql(`drop table if exists "Course_pre_req" cascade;`);

    this.addSql(`drop table if exists "group_entity" cascade;`);

    this.addSql(`drop table if exists "Group_course" cascade;`);

    this.addSql(`drop table if exists "Category_group" cascade;`);

    this.addSql(`drop table if exists "programme" cascade;`);

    this.addSql(`drop table if exists "Programme_Category" cascade;`);
  }

}
