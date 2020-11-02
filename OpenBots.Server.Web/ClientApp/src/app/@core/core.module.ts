import { ModuleWithProviders, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NbAuthModule } from '@nebular/auth';
import { CommonBackendModule } from './common-backend.module';
import { CommonMockModule } from './mock/common/common-mock.module';
import { EcommerceMockModule } from './mock/ecommerce/ecommerce-mock.module';
import { LayoutService } from './utils';
import { AnalyticsService } from './utils/analytics.service';

export const NB_CORE_PROVIDERS = [
  ...CommonMockModule.forRoot().providers,
  ...CommonBackendModule.forRoot().providers,
  ...EcommerceMockModule.forRoot().providers,
  AnalyticsService,
  LayoutService,
];

@NgModule({
  imports: [
    CommonModule,
  ],
  exports: [
    NbAuthModule,
  ],
  declarations: [],
})
export class CoreModule {
  constructor() {
  }

  static forRoot(): ModuleWithProviders<CoreModule> {
    return {
      ngModule: CoreModule,
      providers: [
        ...NB_CORE_PROVIDERS
      ],
    };
  }
}
