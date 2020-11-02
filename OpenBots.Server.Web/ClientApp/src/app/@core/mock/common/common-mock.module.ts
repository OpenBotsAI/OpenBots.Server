import { ModuleWithProviders, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PeriodsService } from './periods.service';

const SERVICES = [
  { provide: PeriodsService, useClass: PeriodsService },
];

@NgModule({
  imports: [CommonModule],
})
export class CommonMockModule {
  static forRoot(): ModuleWithProviders<CommonMockModule> {
    return {
      ngModule: CommonMockModule,
      providers: [
        ...SERVICES,
      ],
    };
  }
}
