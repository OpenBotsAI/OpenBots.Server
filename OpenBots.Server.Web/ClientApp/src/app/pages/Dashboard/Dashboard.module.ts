import { NgModule } from '@angular/core';
import {
  NbButtonModule,
  NbCardModule,
  NbProgressBarModule,
  NbTabsetModule,
  NbUserModule,
  NbIconModule,
  NbSelectModule,
  NbListModule,
} from '@nebular/theme';
import { ThemeModule } from '../../@theme/theme.module';

import { ChartsModule } from 'ng2-charts';
import { DashboardComponent } from './Dashboard.component';
import { BlockUIModule } from 'ng-block-ui';

@NgModule({
  imports: [
    ThemeModule,
    NbCardModule,
    NbUserModule,
    NbButtonModule,
    NbIconModule,
    NbTabsetModule,
    NbSelectModule,
    NbListModule,
    NbProgressBarModule,
    ChartsModule,
    BlockUIModule,
  ],
  declarations: [DashboardComponent],
  providers: [],
})
export class dashboardModule {}
