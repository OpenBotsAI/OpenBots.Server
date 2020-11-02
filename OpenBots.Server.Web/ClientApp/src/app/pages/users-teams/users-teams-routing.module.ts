import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UsersComponent } from './users/users.component';
import { RequestsUserComponent } from './requests-user/requests-user.component';
import { AddUsersTeamComponent } from './add-users-team/add-users-team.component';
 

const routes: Routes = [
  {
    path: 'teams-member',
    component: UsersComponent,
  }, 
  {
    path: 'request-teams',
    component: RequestsUserComponent,
  },
  {
    path: 'add-teams',
    component: AddUsersTeamComponent,
  }

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsersTeamsRoutingModule { }
