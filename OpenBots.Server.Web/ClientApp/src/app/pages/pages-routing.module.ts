import { RouterModule, Routes } from '@angular/router';
import { NgModule } from '@angular/core';
import { PagesComponent } from './pages.component';
import { NotFoundComponent } from './miscellaneous/not-found/not-found.component';
import { LoginGuard } from '../@core/guards/login.guard';
import { TermGuard } from '../@core/guards/term.guard';
import { DashboardComponent } from './Dashboard/Dashboard.component';

const routes: Routes = [
  {
    path: '',
    component: PagesComponent,
    canActivate: [LoginGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
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
        path: 'automation',
        loadChildren: () =>
          import('./automation/automation.module').then(
            (mod) => mod.AutomationModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'refreshhangfire',
        loadChildren: () =>
          import('./refresh-hangfire/refresh-hangfire.module').then(
            (mod) => mod.RefreshHangfireModule
          ),
        canActivate: [LoginGuard],
      },

      {
        path: 'automationLogs',
        loadChildren: () =>
          import('./automation-logs/automation-logs.module').then(
            (mod) => mod.AutomationLogsModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'system-event',
        loadChildren: () =>
          import('./system-event/system-event.module').then(
            (mod) => mod.SyetemEventModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'integration-logs',
        loadChildren: () =>
          import('./integration-logs/integration-logs.module').then(
            (mod) => mod.IntegrationLogsModule
          ),
        canActivate: [LoginGuard],
      },
      {
        path: 'subscription',
        loadChildren: () =>
          import('./subscription/subscription.module').then(
            (mod) => mod.SubscriptionModule
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
        path: 'config',
        loadChildren: () =>
          import('./config-value/config-value.module').then(
            (mod) => mod.ConfigValueModule
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
