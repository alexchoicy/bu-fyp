import { Migration } from '@mikro-orm/migrations';

export class Migration20251107154601 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`alter table "group_entity" drop column "min_credit";`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table "group_entity" add column "min_credit" int not null;`);
  }

}
