import { NgModule } from '@angular/core';
import { UsersTeamsRoutingModule } from './users-teams-routing.module';
import { UsersComponent } from './users/users.component';
import { RequestsUserComponent } from './requests-user/requests-user.component';
import { UsersTeamService } from './users-team.service';
import { AddUsersTeamComponent } from './add-users-team/add-users-team.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';

@NgModule({
  declarations: [UsersComponent, RequestsUserComponent, AddUsersTeamComponent],
  imports: [
    SharedModule,
    UsersTeamsRoutingModule,
    NgxPaginationModule,
  ],
  providers: [UsersTeamService],
})
export class UsersTeamsModule {}
