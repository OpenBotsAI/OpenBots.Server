import { Component, OnInit } from '@angular/core';
import { HttpService } from '../../@core/services/http.service';
import { Chart } from 'chart.js';
import { BlockUI, NgBlockUI } from 'ng-block-ui';
import { automationsApiUrl } from '../../webApiUrls/automations';
import { JobsApiUrl } from '../../webApiUrls/jobsUrl';
import { AgentApiUrl, QueueItemsApiUrl } from '../../webApiUrls';

@Component({
  selector: 'ngx-Dashboard',
  styleUrls: ['./Dashboard.component.scss'],
  templateUrl: './Dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  @BlockUI() blockUI: NgBlockUI;
  topPage: any;
  pageMore = false;
  allProcess = [];
  showCountjob = [];
  totalAgents: number;
  orgName: string;
  dataProcess: number;
  dataJobs = [];
  dataQueue = [];
  totalCount: any = [];
  showMorebtn: boolean = false;

  count = 0;
  donutCount = 0;

  statusColorArr: { name: string; value: string }[] = [
    { name: 'New', value: '#dc3545' },
    { name: 'Assigned', value: '#6610f2' },
    { name: 'In Progress', value: '#7AE2E2' },
    { name: 'Completed', value: '#FF8BA4' },
    { name: 'Failed', value: '#FFE29A' },
    { name: 'Abandoned', value: 'red' },
    { name: 'Total Jobs', value: '#364380' },
  ];
  chart: any = [];
  public chartClicked({
    event,
    active,
  }: {
    event: MouseEvent;
    active: {}[];
  }): void {}

  public chartHovered({
    event,
    active,
  }: {
    event: MouseEvent;
    active: {}[];
  }): void {}

  constructor(private httpService: HttpService) {}

  ngOnInit() {
    if (localStorage.getItem('ActiveOrgname')) {
      this.orgName = localStorage.getItem('ActiveOrgname');
    }
    this.showTotalProcess();
    this.showTotalJob();
    this.getTotalAgents();
    this.showCountQueue();
    this.loadmore(false);
  }

  loadmore(bool: boolean) {
    if (!bool) {
      const top = 6;
      this.showAllProcess(top, this.count);
    } else if (bool) {
      const top = 6;
      const skip = this.count + top;
      this.count = skip;
      this.showAllProcess(top, this.count);
    }
  }

  showTotalProcess(): void {
    this.httpService
      .get(`${automationsApiUrl.automations}/${automationsApiUrl.count}`)
      .subscribe((processData: number) => {
        if (processData || processData === 0) this.dataProcess = processData;
      });
  }

  showTotalJob(): void {
    this.httpService
      .get(`${JobsApiUrl.jobs}/${JobsApiUrl.count}`)
      .subscribe((JobsData: any) => {
        if (JobsData || JobsData === 0) this.dataJobs = JobsData;
      });
  }
  getTotalAgents(): void {
    this.httpService
      .get(`${AgentApiUrl.Agents}/${AgentApiUrl.count}`)
      .subscribe((agentsData: any) => {
        if (agentsData || agentsData === 0) this.totalAgents = agentsData;
      });
  }

  showCountQueue(): void {
    this.httpService
      .get(`${QueueItemsApiUrl.QueueItems}/${QueueItemsApiUrl.count}`)
      .subscribe((countQueue: any) => {
        if (countQueue || countQueue === 0) this.dataQueue = countQueue;
      });
  }

  showAllProcess(top: number, skip: number) {
    // this.blockUI.start('Loading');
    let getprocessUrlbyId = `${automationsApiUrl.automations}?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(getprocessUrlbyId).subscribe((allprocess: any) => {
      this.allProcess = allprocess.items;
      this.totalCount = allprocess.totalCount;
      for (let process of this.allProcess) {
        this.httpService
          .get(
            `${JobsApiUrl.jobs}/${JobsApiUrl.CountByStatus}?$filter= AutomationId eq guid'${process.id}'`
          )
          .subscribe((jobcount: any) => {
            this.donutCount++;
            this.demoCountByStatusGraph(
              jobcount,
              this.donutCount,
              process.name
            );
          });
        // this.blockUI.stop();
      }
    });
  }

  demoCountByStatusGraph(job, count, name) {
    this.showMorebtn = true;
    let colorArr = [];
    const keys = Object.entries(job).map(([key, value]) => key);
    const values = Object.entries(job).map(([key, value]) => value);
    for (let key of keys) {
      for (let data of this.statusColorArr) {
        if (data.name == key) {
          colorArr.push(data.value);
        }
      }
    }

    const divCol = document.createElement('div');
    divCol.classList.add('col-md-4');
    let canvas = document.createElement('canvas');
    canvas = divCol.appendChild(canvas);
    canvas.setAttribute('id', `chart-${count}`);
    let ptag = document.createElement('p');
    ptag.innerHTML = name;
    ptag.style.textAlign = 'center';
    ptag = divCol.appendChild(ptag);
    ptag.setAttribute('id', `p-${count}`);
    document.getElementById('charts').appendChild(divCol);
    // var divCol = document.createElement('div');
    // divCol.classList.add('col-md-4');
    // var canvas = document.createElement('canvas');
    // var canvas = divCol.appendChild(canvas);
    // canvas.setAttribute('id', `chart-${count}`);
    // var ptag = document.createElement('p');
    // ptag.innerHTML = name;
    // ptag.style.textAlign = 'center';
    // var ptag = divCol.appendChild(ptag);
    // ptag.setAttribute('id', `p-${count}`);
    // document.getElementById('charts').appendChild(divCol);
    this.chart = new Chart(`chart-${count}`, {
      type: 'doughnut',
      data: {
        labels: keys,
        datasets: [
          {
            label: 'Jobs',
            data: values,
            backgroundColor: colorArr,
          },
        ],
      },
      options: {
        legend: {
          display: false,
          // labels: {
          //   fontColor: 'rgb(255, 99, 132)',
          // },
        },
        animation: {
          easing: 'linear',
          // easing: 'easeInSine',
          // easing: 'easeOutSine',
          // easing: 'easeOutCirc',
          // duration: 1000,
          animateScale: true,
        },
      },
    });
    if (this.totalCount == this.donutCount) {
      this.pageMore = true;
    }
  }
}
