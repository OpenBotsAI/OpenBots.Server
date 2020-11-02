import { Component, OnInit } from '@angular/core';
import { HttpService } from '../../@core/services/http.service';
import { Chart } from 'chart.js';

@Component({
  selector: 'ngx-ecommerce',
  styleUrls: ['./e-commerce.component.scss'],
  templateUrl: './e-commerce.component.html',
})
export class ECommerceComponent implements OnInit {
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
  /// end //

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
    this.httpService.get('Processes/count').subscribe((processData: number) => {
      if (processData) this.dataProcess = processData;
    });
  }

  showTotalJob(): void {
    this.httpService.get('Jobs/count').subscribe((JobsData: any) => {
      if (JobsData) this.dataJobs = JobsData;
    });
  }
  getTotalAgents(): void {
    this.httpService.get('agents/count').subscribe((agentsData: any) => {
      if (agentsData) this.totalAgents = agentsData;
    });
  }

  showCountQueue(): void {
    this.httpService.get('QueueItems/count').subscribe((countQueue: any) => {
      if (countQueue) this.dataQueue = countQueue;
    });
  }

  showAllProcess(top: number, skip: number) {
    let getprocessUrlbyId = `processes?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(getprocessUrlbyId).subscribe((allprocess: any) => {
      this.allProcess = allprocess.items;
      this.totalCount = allprocess.totalCount;
      for (let process of this.allProcess) {
        this.httpService
          .get(`Jobs/CountByStatus?$filter= ProcessId eq guid'${process.id}'`)
          .subscribe((jobcount: any) => {
            this.donutCount++;
            this.demoCountByStatusGraph(
              jobcount,
              this.donutCount,
              process.name
            );
          });
      }
    });
  }

  demoCountByStatusGraph(job, count, name) {
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
    var divCol = document.createElement('div');
    divCol.classList.add('col-md-4');
    var canvas = document.createElement('canvas');
    var canvas = divCol.appendChild(canvas);
    canvas.setAttribute('id', `chart-${count}`);
    var ptag = document.createElement('p');
    ptag.innerHTML = name;
    ptag.style.textAlign = 'center';
    var ptag = divCol.appendChild(ptag);
    ptag.setAttribute('id', `p-${count}`);
    document.getElementById('charts').appendChild(divCol);
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
      },
    });
    if (this.totalCount == this.donutCount) {
      this.pageMore = true;
    }
  }
}
