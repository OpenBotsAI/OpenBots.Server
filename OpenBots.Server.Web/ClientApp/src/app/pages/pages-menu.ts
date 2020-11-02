/*
 * Copyright (c) Akveo 2019. All Rights Reserved.
 * Licensed under the Single Application / Multi Application License.
 * See LICENSE_SINGLE_APP / LICENSE_MULTI_APP in the 'docs' folder for license information on type of purchased license.
 */

import { NbIconLibraries, NbMenuItem } from '@nebular/theme';
import { Observable, of } from 'rxjs';
import { Injectable } from '@angular/core';
@Injectable()
export class PagesMenu {
  iconDate: any;
  constructor(private iconLibraries: NbIconLibraries) {
    this.iconLibraries.registerSvgPack('custom-icons', {
      agent: '<img src="../../assets/icons/Agent.svg">',
      asset: '<img src="../../assets/icons/Asset.svg">',
      credential: '<img src="../../assets/icons/Credential.svg">',
      email: '<img src="../../assets/icons/mail.svg">',
      dashboard: '<img src="../../assets/icons/Dashboard.svg">',
      file: '<img src="../../assets/icons/File.svg">',
      job: '<img src="../../assets/icons/Job.svg">',
      log: '<img src="../../assets/icons/Log.svg">',
      process: '<img src="../../assets/icons/Process.svg">',
      queue: '<img src="../../assets/icons/Queue.svg">',
      schedule: '<img src="../../assets/icons/Schedule.svg">',
      team: '<img src="../../assets/icons/Team.svg">',
    });
  }
  getMenu(): Observable<NbMenuItem[]> {
    const dashboardMenu: NbMenuItem[] = [
      {
        title: 'Dashboard',
        icon: { icon: 'dashboard', pack: 'custom-icons' },
        link: '/pages/dashboard',
        home: true,
        children: undefined,
      },
    ];

    const menu: NbMenuItem[] = [
      {
        title: 'Agents',
        icon: { icon: 'agent', pack: 'custom-icons' },
        children: [
          {
            title: 'All Agents',
            link: '/pages/agents/list',
          },

          {
            title: 'Add Agent',
            link: '/pages/agents/new',
          },
        ],
      },

      {
        title: 'Assets',
        icon: { icon: 'asset', pack: 'custom-icons' },
        children: [
          {
            title: 'All Assets',
            link: '/pages/asset/list',
          },

          {
            title: 'Add Asset',
            link: '/pages/asset/add',
          }
        ],
      },

      {
        title: 'Change History',
        icon: { icon: 'log', pack: 'custom-icons' },
        children: [
          {
            title: 'All Change History',
            link: '/pages/change-log/list',
          },
        ],
      },
      {
        title: 'Credentials',
        icon: { icon: 'credential', pack: 'custom-icons' },
        children: [
          {
            title: 'All Credentials',
            link: '/pages/credentials',
          },
          {
            title: 'Add Credential',
            link: '/pages/credentials/add',
          },
        ],
      },

      {
        title: 'Emails',
        icon: { icon: 'email', pack: 'custom-icons' },
        children: [
          {
            title: 'All Email Accounts',
            link: '/pages/emailaccount/list',
          },
          {
            title: 'Add Email Account',
            link: '/pages/emailaccount/add',
          },
          {
            title: 'All Email Logs',
            link: '/pages/emaillog/list',
          },
          {
            title: 'Settings',
            link: '/pages/emailsetting/list',
          },
        ],
      },

      {
        title: 'Files',
        icon: { icon: 'file', pack: 'custom-icons' },
        children: [
          {
            title: 'All Files',
            link: '/pages/file/list',
          },
          {
            title: 'Add File',
            link: '/pages/file/add',
          },
        ],
      },
      {
        title: 'Jobs',
        icon: { icon: 'job', pack: 'custom-icons' },
        children: [
          {
            title: 'All Jobs',
            link: '/pages/job/list',
          },
        ],
      },
      {
        title: 'Processes',
        icon: { icon: 'process', pack: 'custom-icons' },
        children: [
          {
            title: 'All Processes',
            link: '/pages/process/list',
          },
          {
            title: 'Add Process',
            link: '/pages/process/add',
          },
          {
            title: 'All Process Logs',
            link: '/pages/processlogs',
          },
        ],
      },

      {
        title: 'Queues',
        icon: { icon: 'queue', pack: 'custom-icons' },
        children: [
          {
            title: 'All Queue Items',
            link: '/pages/queueitems',
          },
          {
            title: 'Add Queue Item',
            link: '/pages/queueitems/new',
          },
          {
            title: 'All Queues',
            link: '/pages/queueslist',
          },
          {
            title: 'Add Queue',
            link: '/pages/queueslist/add',
          },
        ],
      },
      {
        title: 'Schedules',
        icon: { icon: 'schedule', pack: 'custom-icons' },
        children: [
          {
            title: 'All Schedules',
            link: '/pages/schedules',
          },
          {
            title: 'Add Schedule',
            link: '/pages/schedules/add',
          },
        ],
      },

      {
        title: 'Team',
        icon: { icon: 'team', pack: 'custom-icons' },
        children: [
          {
            title: 'All Team Members',
            link: '/pages/users/teams-member',
          },
          {
            title: 'Pending Approvals',
            link: '/pages/users/request-teams',
          },
        ],
      },
    ];
    return of([...dashboardMenu, ...menu]);
  }
}
