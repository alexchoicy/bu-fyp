import { Migration } from '@mikro-orm/migrations';

export class Migration20251107174037 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`alter table "category" add column "priority" int not null;`);
  }

  override async down(): Promise<void> {
    this.addSql(`alter table "category" drop column "priority";`);
  }

}
