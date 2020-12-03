/*
 * Copyright (c) Akveo 2019. All Rights Reserved.
 * Licensed under the Single Application / Multi Application License.
 * See LICENSE_SINGLE_APP / LICENSE_MULTI_APP in the 'docs' folder for license information on type of purchased license.
 */

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
