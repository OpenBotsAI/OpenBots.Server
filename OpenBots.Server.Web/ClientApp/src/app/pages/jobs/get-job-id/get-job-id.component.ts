import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { JobsService } from '../jobs.service';
import { DatePipe } from '@angular/common';
import { FormGroup, FormBuilder } from '@angular/forms';
import { SignalRService } from '../../../@core/services/signal-r.service';
import { environment } from '../../../../environments/environment';
import * as signalR from '@aspnet/signalr';
import { NbToastrService } from '@nebular/theme';

@Component({
  selector: 'ngx-get-job-id',
  templateUrl: './get-job-id.component.html',
  styleUrls: ['./get-job-id.component.scss'],
})
export class GetJobIdComponent implements OnInit, OnDestroy {
  params_id: any = [];
  jsonValue: any = [];
  show_alljobs: any = [];
  showjobs: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  jobStatus: { id: number; name: string }[] = [
    { id: 0, name: 'Unknown' },
    { id: 1, name: 'New' },
    { id: 2, name: 'Assigned' },
    { id: 3, name: 'InProgress' },
    { id: 4, name: 'Failed' },
    { id: 5, name: 'Completed' },
    { id: 9, name: 'Abandoned' },
  ];
  show_createdon: any = [];
  hubConnection: signalR.HubConnection;

  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    protected router: Router,
    private toastrService: NbToastrService,
    public signalRService: SignalRService,
    protected jobService: JobsService
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.params_id = params.id;
      this.get_job(params.id);
    });
  }

  ngOnInit(): void {
    this.showjobs = this.formBuilder.group({
      agentId: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      dequeueTime: [''],
      endTime: [''],
      enqueueTime: [''],
      id: [''],
      isDeleted: [''],
      isSuccessful: [''],
      jobStatus: [''],
      message: [''],
      processId: [''],
      startTime: [''],
      timestamp: [''],
      updatedBy: [''],
      agentName: [''],
      processName: [''],
    });
    this.startConnection();
  }

  private startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl)
      .build();
    this.hubConnection
      .start()
      .then(() => {
        this.hubConnection.on('sendjobnotification', (data) => {
          this.toastrService.success('update the values on the screen');
          this.get_job(this.params_id);
        });
      })
      .catch((err) => {
        this.toastrService.danger(err);
      });
  };

  closeHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop().then();
      this.hubConnection = undefined;
    }
  }

  ngOnDestroy(): void {
    this.closeHubConnection();
  }

  get_job(id) {
    this.jobService.getJobsId(id).subscribe((data: any) => {
      this.show_alljobs = data;
      for (let status of this.jobStatus) {
        if (data.jobStatus == status.id) {
          data.jobStatus = status.name;
        }
      }
      const datepipe: DatePipe = new DatePipe('en-US');
      data.endTime = datepipe.transform(data.endTime, 'MM-dd-yyyy HH:mm:ss a');
      data.startTime = datepipe.transform(
        data.startTime,
        'MM-dd-yyyy HH:mm:ss a'
      );
      data.enqueueTime = datepipe.transform(
        data.enqueueTime,
        'MM-dd-yyyy HH:mm:ss a'
      );
      data.dequeueTime = datepipe.transform(
        data.dequeueTime,
        'MM-dd-yyyy HH:mm:ss a'
      );

      this.showjobs.patchValue(data);
      this.showjobs.disable();
    });
  }

  gotoprocess() {
    this.router.navigate(['/pages/process/get-process-id'], {
      queryParams: { id: this.showjobs.value.processId },
    });
  }
  gotoagent() {
    this.router.navigate(['/pages/agents/get-agents-id'], {
      queryParams: { id: this.showjobs.value.agentId },
    });
  }
  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.Job', id: this.params_id } })
  }
}
