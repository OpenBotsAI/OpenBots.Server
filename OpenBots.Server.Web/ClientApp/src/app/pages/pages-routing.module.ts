import { RouterModule, Routes } from '@angular/router';
import { NgModule } from '@angular/core';
import { PagesComponent } from './pages.component';
import { NotFoundComponent } from './miscellaneous/not-found/not-found.component';
import { ECommerceComponent } from './e-commerce/e-commerce.component';
import { LoginGuard } from '../@core/guards/login.guard';
import { TermGuard } from '../@core/guards/term.guard';

const routes: Routes = [
  {
    path: '',
    component: PagesComponent,
    canActivate: [LoginGuard],
    children: [
      {
        path: 'dashboard',
        component: ECommerceComponent,
        canActivate: [TermGuard, LoginGuard],
      },

      {
        path: 'users',
        loadChildren: () =>
          import('./users-teams/users-teams.module').then(
            (m) => m.UsersTeamsModule
          ),
        canActivate: [LoginGuard],
      },

      {
        path: 'agents',
        loadChildren: () =>
          import('./agents/agents.module').then((m) => m.AgentsModule),
        canActivate: [LoginGuard],
      },

      {
        path: 'queueitems',
        loadChildren: () =>
          import('./queue-items/queue-items.module').then(
            (mod) => mod.QueueItemsModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'change-log',
        loadChildren: () =>
          import('./change-log/change-log.module').then(
            (mod) => mod.ChangelogModule
          ),
        canActivate: [LoginGuard],
      },

      {
        path: 'emailaccount',
        loadChildren: () =>
          import('./email-account/email-account.module').then(
            (mod) => mod.EmailAccountModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'emaillog',
        loadChildren: () =>
          import('./email-log/email-log.module').then(
            (mod) => mod.EmailLogModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'emailsetting',
        loadChildren: () =>
          import('./emailsetting/emailsetting.module').then(
            (mod) => mod.EmailsettingModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'file',
        loadChildren: () =>
          import('./file/files.module').then((mod) => mod.FileModule),
        canActivate: [LoginGuard],
      },
      {
        path: 'asset',
        loadChildren: () =>
          import('./asset/asset.module').then((mod) => mod.AssestModule),
        canActivate: [LoginGuard],
      },
      {
        path: 'job',
        loadChildren: () =>
          import('./jobs/jobs.module').then((mod) => mod.JobsModule),
        canActivate: [LoginGuard],
      },
      {
        path: 'process',
        loadChildren: () =>
          import('./process/process.module').then((mod) => mod.ProcessModule),
        canActivate: [LoginGuard],
      },
      {
        path: 'refreshhangfire',
        loadChildren: () =>
          import('./refresh-hangfire/refresh-hangfire.module').then((mod) => mod.RefreshHangfireModule),
        canActivate: [LoginGuard],
      },

      {
        path: 'processlogs',
        loadChildren: () =>
          import('./process-logs/process-logs.module').then(
            (mod) => mod.ProcessLogsModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'credentials',
        loadChildren: () =>
          import('./credentials/credentials.module').then(
            (mod) => mod.CredentialsModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'schedules',
        loadChildren: () =>
          import('./schedule/schedule.module').then(
            (mod) => mod.ScheduleModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'queueslist',
        loadChildren: () =>
          import('./queues/queues.module').then((mod) => mod.QueuesModule),
        canActivate: [LoginGuard],
      },
      {
        path: 'miscellaneous',
        loadChildren: () =>
          import('./miscellaneous/miscellaneous.module').then(
            (m) => m.MiscellaneousModule
          ),
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
        canActivate: [LoginGuard],
      },
      {
        path: '**',
        component: NotFoundComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PagesRoutingModule { }
