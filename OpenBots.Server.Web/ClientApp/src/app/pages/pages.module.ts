import { NgModule } from '@angular/core';
import { PagesComponent } from './pages.component';
import { PagesRoutingModule } from './pages-routing.module';
import { ThemeModule } from '../@theme/theme.module';
import { MiscellaneousModule } from './miscellaneous/miscellaneous.module';
import { PagesMenu } from './pages-menu';
import { NbMenuModule } from '@nebular/theme';
import { NgxPaginationModule } from 'ngx-pagination';
import { dashboardModule } from './Dashboard/Dashboard.module';

const PAGES_COMPONENTS = [PagesComponent];

@NgModule({
  imports: [
    PagesRoutingModule,
    ThemeModule,
    dashboardModule,
    NbMenuModule,
    MiscellaneousModule,
    NgxPaginationModule,
  ],
  declarations: [...PAGES_COMPONENTS],
  providers: [PagesMenu],
})
export class PagesModule {}
