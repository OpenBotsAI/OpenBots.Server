import { NgModule } from '@angular/core';
import { PagesComponent } from './pages.component';
import { PagesRoutingModule } from './pages-routing.module';
import { ThemeModule } from '../@theme/theme.module';
import { MiscellaneousModule } from './miscellaneous/miscellaneous.module';
import { PagesMenu } from './pages-menu';
import { ECommerceModule } from './e-commerce/e-commerce.module';
import { NbMenuModule } from '@nebular/theme';
import { AuthModule } from '../@auth/auth.module';
import { NgxPaginationModule } from 'ngx-pagination';

const PAGES_COMPONENTS = [PagesComponent];

@NgModule({
  imports: [
    PagesRoutingModule,
    ThemeModule,
    ECommerceModule,
    NbMenuModule,
    MiscellaneousModule,
    NgxPaginationModule,
  ],
  declarations: [...PAGES_COMPONENTS],
  providers: [PagesMenu],
  
})
export class PagesModule {}
